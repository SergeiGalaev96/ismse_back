using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models
{
    public static class DAL
    {

        public static string meta_DB_connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["cissaMetaDb"].ConnectionString;
        public static string data_DB_connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["cissaDataDb"].ConnectionString;

        public static string GetClassName(Guid id)
        {
            SqlConnection sqlConnection1 = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            cmd.CommandText = "SELECT Name FROM object_defs WHERE id='" + id.ToString() + "'";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection1;

            sqlConnection1.Open();
            string className = cmd.ExecuteScalar().ToString();
            sqlConnection1.Close();

            return className;
        }

        public static string GetClassName1(Guid id)
        {
            SqlConnection sqlConnection1 = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            cmd.CommandText = "SELECT Name FROM object_defs WHERE id='" + id.ToString() + "'";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection1;

            sqlConnection1.Open();
            string className = cmd.ExecuteScalar().ToString();
            sqlConnection1.Close();

            return className;
        }

        public class MenuItem
        {
            public Guid Id { get; set; }
            public string NameEN { get; set; }
            public string NameRU { get; set; }
            public int OrderIndex { get; set; }
            public List<MenuItem> Children { get; set; }
        }
        public class Item
        {
            public Guid? parentId { get; set; }
            public Guid elementId { get; set; }
            public string NameEN { get; set; }
            public string NameRU { get; set; }
            public int OrderIndex { get; set; }
        }
        public static List<MenuItem> GetMenuItems()
        {
            var items = new List<Item>();
            SqlConnection sqlConnection1 = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            cmd.CommandText = @"
select od.Parent_Id 'ParentId', od.Id 'ElementId', od.Name 'NameEN', od.Full_Name 'NameRU', od.Order_Index
from Menus m
inner join Object_Defs od on (od.Id = m.Id)
where od.Full_Name is not null";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection1;

            sqlConnection1.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var parentId = reader.IsDBNull(0) ? null : (Guid?)reader.GetGuid(0);
                    var elementId = reader.IsDBNull(1) ? Guid.Empty : reader.GetGuid(1);
                    var nameEng = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    var nameRus = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                    var orderIndex = reader.IsDBNull(4) ? 0 : reader.GetInt16(4);
                    items.Add(new Item
                    {
                        elementId = elementId,
                        parentId = parentId,
                        NameEN = nameEng,
                        NameRU = nameRus,
                        OrderIndex = orderIndex
                    });
                }
            }



            sqlConnection1.Close();

            return ConvertItemsToMenu(items);
        }
        public class _cissa_user
        {
            public Guid Id { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string OrgName { get; set; }
        }
        public static List<_cissa_user> GetCissaUsers()
        {
            var items = new List<_cissa_user>();
            SqlConnection sqlConnection1 = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            cmd.CommandText = @"
    SELECT users.Id, users.User_Name, users.User_Password, od2.Full_Name
    FROM Workers users
	inner join Object_Defs od on od.Id = users.Id
	inner join Object_Defs od2 on od.Parent_Id = od2.Id
    where users.User_Name not like '%*%'
";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection1;

            sqlConnection1.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var userId = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
                    var userName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    var uPassword = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    var uOrgName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                    items.Add(new _cissa_user
                    {
                        Id = userId,
                        UserName = userName,
                        Password = uPassword,
                        OrgName = uOrgName
                    });
                }
            }



            sqlConnection1.Close();

            return items;
        }

        public static _cissa_user GetCissaUser(Guid userId)
        {
            var items = new List<_cissa_user>();
            using (SqlConnection sqlConnection = new SqlConnection(meta_DB_connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                //SqlDataReader reader;

                cmd.CommandText = @"
    SELECT users.Id, users.User_Name, users.User_Password
    FROM Workers users
    where users.Id = '" + userId + @"'
";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection;
                var uObj = new _cissa_user();
                sqlConnection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
                        var userName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        var uPassword = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                        uObj.Id = id;
                        uObj.UserName = userName;
                        uObj.Password = uPassword;
                        return uObj;
                    }
                }

                sqlConnection.Close();
            }
            throw new InvalidOperationException($"Пользователь с UserId:{userId} не найден на сервере {System.Configuration.ConfigurationManager.AppSettings["msecdb"]}");
        }

        private static List<MenuItem> ConvertItemsToMenu(List<Item> items)
        {
            var menuItems = new List<MenuItem>();

            foreach (var item in items.Where(x => x.parentId == null))
            {
                var mItem = new MenuItem
                {
                    Id = item.elementId,
                    NameEN = item.NameEN,
                    NameRU = item.NameRU,
                    OrderIndex = item.OrderIndex,
                    Children = new List<MenuItem>()
                };
                InitChildren(items, mItem);
                menuItems.Add(mItem);
            }

            return menuItems;
        }
        private static void InitChildren(List<Item> items, MenuItem element)
        {
            foreach (var subItem in items.Where(x => x.parentId == element.Id))
            {
                var subMenuItem = new MenuItem
                {
                    Id = subItem.elementId,
                    NameEN = subItem.NameEN,
                    NameRU = subItem.NameRU,
                    OrderIndex = subItem.OrderIndex,
                    Children = new List<MenuItem>()
                };
                InitChildren(items, subMenuItem);
                element.Children.Add(subMenuItem);
            }
        }

        public class Organization
        {
            public Guid Id { get; set; }
            public string Text { get; set; }

        }

        public static List<Organization> GetOrganizationList()
        {
            var organizations = new List<Organization>();
            SqlConnection sqlConnection1 = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            cmd.CommandText = @"
                SELECT o.[Id], od.Full_Name FROM [dbo].[Organizations] o
                INNER JOIN Object_Defs od ON o.Id = od.Id AND od.Parent_Id = '584F67EA-DADD-406F-95D5-45FFBE98853C'";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection1;

            sqlConnection1.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var Id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
                    var Text = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);

                    organizations.Add(new Organization
                    {
                        Id = Id,
                        Text = Text
                    });
                }
            }

            sqlConnection1.Close();

            return organizations;
        }
        public class User
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public Guid ParentId { get; set; }
            public Guid OrgPositionId { get; set; }
            public Guid UserId { get; set; }
            public bool? Sex { get; set; }
            public int LanguageId { get; set; }
        }

        public static bool IsExistWorker(Guid userId)
        {
            SqlConnection sqlConnection = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            cmd.CommandText = @"
                SELECT users.Id
                FROM Workers users
                where users.Id = '" + userId + @"'
";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;
            sqlConnection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return true;
                }
            }

            sqlConnection.Close();
            return false;
        }
        public static bool IsExistUserName(string userName)
        {
            SqlConnection sqlConnection = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            //SqlDataReader reader;

            cmd.CommandText = @"
                SELECT users.Id
                FROM Workers users
                where users.User_Name = '" + userName + @"'
";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;
            sqlConnection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return true;
                }
            }

            sqlConnection.Close();
            return false;
        }

        public static Guid GetUserIdByName(string userName)
        {
            SqlConnection sqlConnection = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = @"
                SELECT users.Id, users.User_Name, users.User_Password
                FROM Workers users
                where users.User_Name = '" + userName + @"'
";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;
            sqlConnection.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0); ;
                }
            }

            sqlConnection.Close();
            throw new InvalidOperationException("User not found");
        }

        public static void CreateObjectDefs(Guid UserId, string UserName, Guid ParentId)
        {
            SqlConnection sqlConnection = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = @"
            INSERT INTO Object_Defs
            ([Id], [Full_Name], [Order_Index], [Parent_Id], [Created])
            VALUES
            ('" + UserId + "','" + UserName + "'," + 0 + ",'" + ParentId + "','" + DateTime.Now.ToString("yyyyMMdd") + "');";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;
            sqlConnection.Open();
            cmd.ExecuteNonQuery();
            sqlConnection.Close();
        }

        public static void CreateSubjects(Guid UserId, string Address, string Phone, string Email)
        {
            SqlConnection sqlConnection = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = @"
            INSERT INTO Subjects
            ([Id], [Address], [Phone], [Email])
            VALUES
            ('" + UserId + "','" + Address + "','" + Phone + "','" + Email + "')";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;
            sqlConnection.Open();
            cmd.ExecuteNonQuery();
            sqlConnection.Close();
        }

        public static void CreatePersons(Guid UserId, string LastName, string FirstName, string MiddleName)
        {
            SqlConnection sqlConnection = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = @"
            INSERT INTO Persons
            ([Id], [Last_Name], [First_Name], [Middle_Name])
            VALUES
            ('" + UserId + "','" + LastName + "','" + FirstName + "','" + MiddleName + "')";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;
            sqlConnection.Open();
            cmd.ExecuteNonQuery();
            sqlConnection.Close();
        }
        public static void CreateWorkers(Guid UserId, string UserName, string Password, Guid OrgPositionId, int LanguageId)
        {
            SqlConnection sqlConnection = new SqlConnection(meta_DB_connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = @"
            INSERT INTO Workers
            ([Id], [User_Name], [User_Password], [OrgPosition_Id], [Language_Id])
            VALUES
            ('" + UserId + "','" + UserName + "','" + Password + "','" + OrgPositionId + "'," + LanguageId + ")";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;
            sqlConnection.Open();
            cmd.ExecuteNonQuery();
            sqlConnection.Close();
        }


        public class CreateUserResponse
        {
            public Guid? UserId { get; set; }
            public int Status { get; set; }
            public string Message { get; set; }
        }
    }
}