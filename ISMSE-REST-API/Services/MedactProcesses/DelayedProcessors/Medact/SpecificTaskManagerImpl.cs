using ISMSE_REST_API.Contracts.Delegates;
using ISMSE_REST_API.Contracts.Infrastructure;
using ISMSE_REST_API.Contracts.Infrastructure.Logging;
using ISMSE_REST_API.Contracts.MedactProcesses.DelayedProcessors.Medact;
using ISMSE_REST_API.Contracts.Scheduler;
using ISMSE_REST_API.Models.Delayed;
using ISMSE_REST_API.Models.Exceptions;
using Raven.Imports.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace ISMSE_REST_API.Services.MedactProcesses.DelayedProcessors.Medact
{
    public class SpecificTaskManagerImpl : ISpecificTaskManager
    {
        private readonly IProcessScheduler _processScheduler;
        private readonly IConverterPresenter _converterPresenter;
        private readonly IExpiredMedactProcessor _expiredMedactProcessor;
        private readonly ILogManager _logManager;
        public SpecificTaskManagerImpl(IProcessScheduler processScheduler, IExpiredMedactProcessor expiredMedactProcessor,
            IConverterPresenter converterPresenter, ILogManager logManager)
        {
            _processScheduler = processScheduler;
            _expiredMedactProcessor = expiredMedactProcessor;
            _converterPresenter = converterPresenter;
            _logManager = logManager;
        }
        private static List<SpecificTaskDefinition> SpecificTasks { get; set; } = new List<SpecificTaskDefinition>();

        public SpecificTaskDefinitionInfo[] GetTasks()
        {
            return SpecificTasks.Select(x => x.GetInfo()).ToArray();
        }
        public void ClearTasks()
        {
            _logManager.WriteLog($"All Tasks Clearing started...");
            foreach (var task in SpecificTasks)
            {
                task.TimerObj.Stop();
                task.TimerObj.Dispose();
                _logManager.WriteLog($"Task({task.TaskId}) cancelled");
            }
            _logManager.WriteLog($"All Tasks are Stoped and Disposed");
            //SpecificTasks.Clear();
        }

        public void RunNewTaskMedactCleaner(int? hours = null, int? minutes = null, int? secs = null)
        {
            if (SpecificTasks.Any(x => x.TimerObj.Enabled))
                throw new TaskDuplicateException("Задача уже создана! Необходимо его остановить и удалить, а затем только создать заново");
            var expireMedactScheduleTime = System.Configuration.ConfigurationManager.AppSettings["expireMedactScheduleTime"];
            if (string.IsNullOrEmpty(expireMedactScheduleTime))
                throw new ArgumentNullException($"expireMedactScheduleTime", "Значение не указано в конфигурационном файле");

            hours = hours ?? _converterPresenter.GetHours(expireMedactScheduleTime);
            minutes = minutes ?? _converterPresenter.GetMinutes(expireMedactScheduleTime);
            secs = secs ?? 0;
            var task = new SpecificTaskDefinition(_processScheduler.RecurredJobInSpecificTime(hours.Value, minutes.Value, secs.Value, new ElapsedEventHandler(CallClearExpiredMedacts)));
            
            SpecificTasks.Add(task);
            _logManager.WriteLog($"New Task created with id: {task.TaskId}");
        }
        public void RunOne(Guid taskId)
        {
            var specTaskObj = SpecificTasks.FirstOrDefault(x => x.TaskId == taskId);
            if (specTaskObj != null)
            {
                specTaskObj.TimerObj.Interval = 0;
                specTaskObj.TimerObj.Enabled = false;
                specTaskObj.TimerObj.Enabled = true;
            }
        }
        private void CallClearExpiredMedacts(object sender, ElapsedEventArgs e)
        {
            _logManager.WriteLog($"CallClearExpiredMedacts invoked {e.SignalTime:yyyy-MM-dd HH:mm:ss.fff}");
            try
            {
                var timer = (System.Timers.Timer)sender;
                var specTaskObj = SpecificTasks.FirstOrDefault(x => x.TimerObj.GetHashCode() == timer.GetHashCode());
                if (specTaskObj != null)
                {
                    var taskObj = Task.Factory.StartNew(() =>
                    {
                        //Thread.Sleep(TimeSpan.FromSeconds(4));
                        _logManager.WriteLog("Начинаю очистку");
                        _expiredMedactProcessor.Execute();
                        _logManager.WriteLog("Завершил очистку");
                    });
                    specTaskObj.AddTask(taskObj);
                }
                else
                    throw new InvalidOperationException("Не могу запустить задачу на исполнение.\n" +
                        "Запланированный экземпляр не найден в списке запланированных задач.\n" +
                        $"Timer-hash-code: {timer.GetHashCode()}\n" +
                        $"AvailableSpecTasks: {JsonConvert.SerializeObject(GetTasks())}");
            }
            catch (Exception ex)
            {
                _logManager.WriteLog($"Ошибка при попытке запустить запланированную задачу по наступлению времени исполнения.\n" +
                    $"Детали:\n" +
                    $"ErrorMessage: {ex.Message}\n" +
                    $"trace: {ex.StackTrace}\n" +
                    $""
                    );
            }
        }
    }
}