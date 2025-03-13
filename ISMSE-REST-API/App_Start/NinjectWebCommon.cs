[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(ISMSE_REST_API.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(ISMSE_REST_API.App_Start.NinjectWebCommon), "Stop")]

namespace ISMSE_REST_API.App_Start
{
    using System;
    using System.Web;
    using ISMSE_REST_API.Contracts.Builders;
    using ISMSE_REST_API.Contracts.CustomExporter;
    using ISMSE_REST_API.Contracts.DataProviders;
    using ISMSE_REST_API.Contracts.Delegates;
    using ISMSE_REST_API.Contracts.Documents;
    using ISMSE_REST_API.Contracts.Infrastructure;
    using ISMSE_REST_API.Contracts.Infrastructure.Logging;
    using ISMSE_REST_API.Contracts.MedactProcesses;
    using ISMSE_REST_API.Contracts.MedactProcesses.DelayedProcessors.Medact;
    using ISMSE_REST_API.Contracts.MedactProcesses.Verification;
    using ISMSE_REST_API.Contracts.Notifications;
    using ISMSE_REST_API.Contracts.Scheduler;
    using ISMSE_REST_API.Contracts.Status;
    using ISMSE_REST_API.Models.Enums.Logging;
    using ISMSE_REST_API.Services;
    using ISMSE_REST_API.Services.Builders;
    using ISMSE_REST_API.Services.CustomExporter;
    using ISMSE_REST_API.Services.DataProviders;
    using ISMSE_REST_API.Services.Documents;
    using ISMSE_REST_API.Services.Infrastructure;
    using ISMSE_REST_API.Services.Infrastructure.Logging;
    using ISMSE_REST_API.Services.MedactProcesses;
    using ISMSE_REST_API.Services.MedactProcesses.DelayedProcessors;
    using ISMSE_REST_API.Services.MedactProcesses.DelayedProcessors.Medact;
    using ISMSE_REST_API.Services.MedactProcesses.Verification;
    using ISMSE_REST_API.Services.Notifications;
    using ISMSE_REST_API.Services.Scheduler;
    using ISMSE_REST_API.Services.Status;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Web.Common.WebHost;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application.
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        public static T GetService<T>() => bootstrapper.Kernel.Get<T>();

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<INativeSqlDataProvider>().To<NativeSqlDataProviderImpl>().InRequestScope();
            kernel.Bind<ICustomExporter>().To<CustomExporterImpl>().InRequestScope();
            kernel.Bind<IConverterPresenter>().To<ConverterPresenterImpl>().InRequestScope();
            kernel.Bind<ITSqlQueryBuilder>().To<TSqlQueryBuilderCustomImpl>().InRequestScope();
            kernel.Bind<IExporterFacade>().To<ExporterFacadeImpl>().InRequestScope();
            kernel.Bind<IStatusRepository>().To<StatusRepositoryImpl>().InRequestScope();
            kernel.Bind<ICissaDataAccessLayer>().To<CissaDataAccessLayerImpl>().InRequestScope();
            kernel.Bind<IDataService>().To<DataServiceImpl>().InRequestScope();
            kernel.Bind<ITransferChildToAdult>().To<TransferChildToAdultImpl>().InRequestScope();
            kernel.Bind<IBusinessLogicNotifier>().To<BusinessLogicNotifierImpl>().InRequestScope();
            kernel.Bind<IChildFacade>().To<ChildFacadeImpl>().InRequestScope();
            kernel.Bind<IAdultFacade>().To<AdultFacadeImpl>().InRequestScope();
            kernel.Bind<ChildControl>().ToSelf().InRequestScope();
            kernel.Bind<AdultControl>().ToSelf().InRequestScope();

            kernel.Bind<DuplicateControlServiceResolver>().ToMethod((svc) => (token) =>
            {
                switch (token)
                {
                    case "Child":
                        return svc.Kernel.Get<ChildControl>();
                    case "Adult":
                        return svc.Kernel.Get<AdultControl>();
                    default:
                        throw new InvalidOperationException();
                }
            }).InRequestScope();
            kernel.Bind<IPersonVerification>().To<PersonVerificationImpl>().InRequestScope();

            kernel.Bind<LogWriterInMemoryImpl>().ToSelf().InRequestScope();
            kernel.Bind<LogWriterToFileImpl>().ToSelf().InRequestScope();

            kernel.Bind<LogWriterServiceResolver>().ToMethod((svc) => (logType) =>
            {
                switch (logType)
                {
                    case LogType.MEMORY:
                        return svc.Kernel.Get<LogWriterInMemoryImpl>();
                    case LogType.FILE:
                        return svc.Kernel.Get<LogWriterToFileImpl>();
                    default:
                        throw new InvalidOperationException();
                }
            }).InRequestScope();

            kernel.Bind<IProcessScheduler>().To<ProcessSchedulerImpl>().InRequestScope();
            kernel.Bind<ILogManager>().To<LogManagerImpl>().InSingletonScope();
            kernel.Bind<ISpecificTaskManager>().To<SpecificTaskManagerImpl>().InSingletonScope();
            kernel.Bind<IExpiredMedactProcessor>().To<ExpiredMedactProcessorImpl>().InSingletonScope();
        }
    }
}