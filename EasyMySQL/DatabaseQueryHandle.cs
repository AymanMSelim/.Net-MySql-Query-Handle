using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EasyMySQL
{
    /*
      * Auther: Ayman Mohamed Selim
      * Version: 1.0.0.50
      * Email: AymanM.Selim@hotmail.com
      */

    /// <summary>
    /// This class hadle the excution of all database queries
    /// </summary>
    public partial class DatabaseQueryHandle
    {
        protected const int LIMIT = 100;

        //public static int SystemUserID
        //{
        //    get;
        //    set;
        //}

        static private MySqlConnection MySqlConnection
        {
            get;
            set;
        }

        static private MySqlCommand SqlCommand
        {
            get;
            set;
        }

        static private MySqlDataAdapter DataAdapter
        {
            get;
            set;
        }

        static private DataSet DataSet
        {
            get;
            set;
        }

        /// <summary>
        /// This is the query used in all methods, set it before use any method
        /// </summary>
        static protected string Query
        {
            get;
            set;
        }

        private static bool SecurityCheckQuery(string query)
        {
            if (query.ToLower().Contains("delete") && !query.ToLower().Contains("where"))
            {
                return false;
            }

            if (query.ToLower().Contains("update") && !query.ToLower().Contains("where"))
            {
                return false;
            }

            if (query.ToLower().Contains("alter"))
            {
                return false;
            }

            if (query.ToLower().Contains("create"))
            {
                return false;
            }

            if (query.ToLower().Contains("drop"))
            {
                return false;
            }

            if (query.ToLower().Contains("truncate"))
            {
                return false;
            }

            if (query.ToLower().Contains("rename"))
            {
                return false;
            }

            if (query.ToLower().Contains("modify"))
            {
                return false;
            }

            if (query.ToLower().Contains("grant"))
            {
                return false;
            }

            if (query.ToLower().Contains("--"))
            {
                return false;
            }
            return true;
        }

        static DatabaseQueryHandle()
        {
            MySqlConnection = MyConnection.Connection;
            SqlCommand = new MySqlCommand
            {
                Connection = MySqlConnection
            };
            DataAdapter = new MySqlDataAdapter();
            DataSet = new DataSet();
        }

        /// <summary>
        /// Excute all select query to the database and return query data with it's state
        /// </summary>
        /// <returns>Retrun datatable with true status on success or null, false status and error message on fail</returns>
        static protected ExcuteStateMessage<DataTable> SelectQuery()
        {
            if (!SecurityCheckQuery(Query))
            {
                return new ExcuteStateMessage<DataTable>(false, "Security: this query can't be excuted due to security resons");
            }
            try
            {
                ExcuteStateMessage stateMessage = MyConnection.CheckOpenConnection();
                if (!stateMessage.Status)
                {
                    return new ExcuteStateMessage<DataTable>(false, null, stateMessage.Message);
                }

                SqlCommand.CommandText = Query;
                DataAdapter.SelectCommand = SqlCommand;
                ClearDatasetTables();
                DataAdapter.Fill(DataSet);
                int rowCount = DataSet.Tables[0].Rows.Count;
                if (rowCount == 0)
                {
                    return new ExcuteStateMessage<DataTable>(true, null, "لا يوجد بيانات متطابقة");
                }
                else
                {
                    return new ExcuteStateMessage<DataTable>(true, DataSet.Tables[0]);
                }
            }
            catch (Exception ex)
            {
                return new ExcuteStateMessage<DataTable>(false, "SelectQuery Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Select the first row from the result
        /// </summary>
        /// <returns>Retuen true status with the datarow if the opreation comlete successfully or false, null and error message on error</returns>
        static protected ExcuteStateMessage<DataRow> SelectRowQuery()
        {
            if (!SecurityCheckQuery(Query))
            {
                return new ExcuteStateMessage<DataRow>(false, "Security: this query can't be excuted due to security resons");
            }
            DataRow row;
            try
            {
                #region Check Connection return false with reason in case of connection error
                ExcuteStateMessage stateMessage = MyConnection.CheckOpenConnection();
                if (stateMessage.Status == false)
                {
                    return new ExcuteStateMessage<DataRow>(false, stateMessage.Message);
                }
                #endregion

                SqlCommand.CommandText = Query;
                DataAdapter.SelectCommand = SqlCommand;
                ClearDatasetTables();
                DataAdapter.Fill(DataSet);
                // return null datatable if no rows in the table
                if (DataSet.Tables[0].Rows.Count == 0)
                {
                    return new ExcuteStateMessage<DataRow>(true, null, "لا يوجد بيانات متطابقة");
                }
                else
                {
                    row = DataSet.Tables[0].Rows[0];
                    return new ExcuteStateMessage<DataRow>(true, row);
                }
            }
            catch (Exception ex)
            {
                row = null;
                return new ExcuteStateMessage<DataRow>(false, row, "SelectRow Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Excute all insert, update, delete statements
        /// </summary>
        /// <returns>Retuen ExcuteStateMessage with true status on success or false, error message on error</returns>
        static protected ExcuteStateMessage Excute()
        {
            if (!SecurityCheckQuery(Query))
            {
                return new ExcuteStateMessage(false, "Security: this query can't be excuted due to security resons");
            }
            try
            {
                #region Check Connection return false with reason in case of connection error
                ExcuteStateMessage stateMessage = MyConnection.CheckOpenConnection();
                if (stateMessage.Status == false)
                {
                    return new ExcuteStateMessage(false, stateMessage.Message);
                }
                #endregion

                SqlCommand.CommandText = Query;
                SqlCommand.ExecuteNonQuery();
                return new ExcuteStateMessage(true);
            }
            catch (Exception ex)
            {
                Query = "ROLLBACK";
                ExcuteStateMessage excuteRollBackState = ExcuteRollback();
                if (excuteRollBackState.Status)
                {
                    return new ExcuteStateMessage(false, ex.Message);
                }
                else
                {
                    excuteRollBackState.Message = "Excute Error: " + ex.Message + "\n" + excuteRollBackState.Message;
                    return excuteRollBackState;
                }
            }
        }

        /// <summary>
        /// Excute all insert statements and return the last insert id
        /// </summary>
        /// <returns>Retuen ExcuteStateMessage with true status and last insert id on success or false, null and error message on error</returns>
        static protected ExcuteStateMessage ExcuteReturnInsertID()
        {
            if (!SecurityCheckQuery(Query))
            {
                return new ExcuteStateMessage(false, "Security: this query can't be excuted due to security resons");
            }
            try
            {
                #region Check Connection return false with reason in case of connection error
                ExcuteStateMessage stateMessage = MyConnection.CheckOpenConnection();
                if (stateMessage.Status == false)
                {
                    return new ExcuteStateMessage(false, stateMessage.Message);
                }
                #endregion

                SqlCommand.CommandText = Query;
                SqlCommand.ExecuteNonQuery();
                return new ExcuteStateMessage(true, SqlCommand.LastInsertedId);
            }
            catch (Exception ex)
            {
                Query = "ROLLBACK";
                ExcuteStateMessage excuteRollBackState = ExcuteRollback();
                if (excuteRollBackState.Status)
                {
                    return new ExcuteStateMessage(false, ex.Message);
                }
                else
                {
                    return new ExcuteStateMessage(false, "Excute Exception: " + ex.Message + "\n" + excuteRollBackState.Message);
                }
            }
        }

        static protected ExcuteStateMessage ExcuteRollback()
        {
            if (!SecurityCheckQuery(Query))
            {
                return new ExcuteStateMessage(false, "Security: this query can't be excuted due to security resons");
            }
            try
            {
                #region Check Connection return false with reason in case of connection error
                ExcuteStateMessage stateMessage = MyConnection.CheckOpenConnection();
                if (stateMessage.Status == false)
                {
                    return new ExcuteStateMessage(false, stateMessage.Message);
                }
                #endregion

                SqlCommand.CommandText = Query;
                SqlCommand.ExecuteNonQuery();
                return new ExcuteStateMessage(true);
            }
            catch (Exception ex)
            {
                return new ExcuteStateMessage(false, "RollBack Exception: " + ex.Message);
            }
        }

        static protected ExcuteStateMessage ExcuteScaler()
        {
            if (!SecurityCheckQuery(Query))
            {
                return new ExcuteStateMessage(false, "Security: this query can't be excuted due to security resons");
            }
            object data;
            try
            {
                #region Check Connection return false with reason in case of connection error
                ExcuteStateMessage stateMessage = MyConnection.CheckOpenConnection();
                if (stateMessage.Status == false)
                {
                    return new ExcuteStateMessage(false, stateMessage.Message);
                }
                #endregion

                SqlCommand.CommandText = Query;
                data = SqlCommand.ExecuteScalar();
                if (data == null)
                {
                    ExcuteStateMessage excuteStateMessage = new ExcuteStateMessage(true, null, "لا يوجد نتائح لهذا العملية");
                    excuteStateMessage.HasData = false;
                    return excuteStateMessage;
                }
                else
                {
                    return new ExcuteStateMessage(true, data);
                }
            }
            catch (Exception ex)
            {
                Query = "ROLLBACK";
                ExcuteStateMessage excuteState = ExcuteRollback();
                if (excuteState.Status == true)
                {
                    return new ExcuteStateMessage(false, "ExcuteScaller Exception: " + ex.Message);
                }
                else
                {
                    excuteState.Message = "ExcuteScaller Exception:  " + ex.Message + "\n" + excuteState.Message;
                    return excuteState;
                }
            }
        }

        public static ExcuteStateMessage StartTransaction()
        {
            Query = "START TRANSACTION";
            if (!SecurityCheckQuery(Query))
            {
                return new ExcuteStateMessage(false, "Security: this query can't be excuted due to security resons");
            }
            return Excute();
        }

        public static ExcuteStateMessage Commit()
        {
            Query = "COMMIT";
            if (!SecurityCheckQuery(Query))
            {
                return new ExcuteStateMessage(false, "Security: this query can't be excuted due to security resons");
            }
            return Excute();
        }

        private static void ClearDatasetTables()
        {
            DataSet.Tables.Clear();
        }
    }
}
