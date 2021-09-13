using System;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Configuration;
using log4net;

namespace SqlExtend
{
    public class ADODB
    {
        private static ILog log = LogManager.GetLogger("logger");
        public static void ExecuteNonQuery<T>(object obj) where T : new()
        {
            string table = TypeDescriptor.GetClassName(obj).Split('.')[1];
            string sql = string.Concat(@"INSERT INTO [", table, "]");
            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["LocalDB"].ToString();
                conn.ConnectionString = connString;
                conn.Open();

                List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

                bool FirstColFlag = true;
                string Cols = "(";
                string Values = "(";
                string paras = "";
                foreach (var property in properties)
                {
                    if (property.GetValue(obj) != null)
                    {
                        if (FirstColFlag)
                        {
                            Cols = string.Concat(Cols, "[", property.Name, "]");
                            Values = string.Concat(Values, "@", property.Name); // 這邊可以改用官方成員，取得是@或:  或用Spring切換DB
                            cmd.Parameters.Add(new SqlParameter(string.Concat("@", property.Name), property.GetValue(obj)));
                            paras = string.Concat(paras, property.Name, ": ", property.GetValue(obj));
                            FirstColFlag = false;
                        }
                        else
                        {
                            Cols = string.Concat(Cols, ", [", property.Name, "]");
                            Values = string.Concat(Values, ", @", property.Name); // 這邊可以改用官方成員，取得是@或:  或用Spring切換DB
                            paras = string.Concat(paras, ", ", property.Name, ": ", property.GetValue(obj));
                            cmd.Parameters.Add(new SqlParameter(string.Concat("@", property.Name), property.GetValue(obj)));
                        }
                    }
                    else
                    {
                        switch (property.Name)
                        {
                            case "sys_createdate":
                                if (FirstColFlag)
                                {
                                    Cols = string.Concat(Cols, "[", property.Name, "]");
                                    Values = string.Concat(Values, "SYSDATE()");
                                    FirstColFlag = false;
                                }
                                else
                                {
                                    Cols = string.Concat(Cols, ", [", property.Name, "]");
                                    Values = string.Concat(Values, ", SYSDATE()");
                                }
                                break;
                            case "sys_updatedate":
                                if (FirstColFlag)
                                {
                                    Cols = string.Concat(Cols, "[", property.Name, "]");
                                    Values = string.Concat(Values, "GETDATE()");
                                    FirstColFlag = false;
                                }
                                else
                                {
                                    Cols = string.Concat(Cols, ", [", property.Name, "]");
                                    Values = string.Concat(Values, ", GETDATE()");
                                }
                                break;
                            case "sys_createuser":
                                if (FirstColFlag)
                                {
                                    Cols = string.Concat(Cols, "[", property.Name, "]");
                                    Values = string.Concat(Values, "'SYSTEM'");
                                    FirstColFlag = false;
                                }
                                else
                                {
                                    Cols = string.Concat(Cols, ", [", property.Name, "]");
                                    Values = string.Concat(Values, ", 'SYSTEM'");
                                }
                                break;
                            case "sys_updateuser":
                                if (FirstColFlag)
                                {
                                    Cols = string.Concat(Cols, "[", property.Name, "]");
                                    Values = string.Concat(Values, "'SYSTEM'");
                                    FirstColFlag = false;
                                }
                                else
                                {
                                    Cols = string.Concat(Cols, ", [", property.Name, "]");
                                    Values = string.Concat(Values, ", 'SYSTEM'");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                Cols += ")";
                Values += ")";

                sql = string.Concat(sql, Cols, " VALUES", Values, ";");
                log.Info(string.Concat("\n\t", sql, "\nParamaters: { ", paras, " }"));
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                log.Error(string.Concat("\n", ex.ToString()));
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }
    }
    
}
