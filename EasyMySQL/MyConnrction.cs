using System;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace EasyMySQL
{
    /*
     * Auther: Ayman Mohamed Selim
     * Version: 1.0.0.50
     * Email: AymanM.Selim@hotmail.com
     */

    /// <summary>
    /// This class contain all connetion parameters and methods in addition to net connection checking
    /// </summary>
    public class MyConnection
    {
        #region Variables
        private const int CONNECTION_ATTEMPS_MAX = 2;

        private static int connectionAttempNumber = 0;
        private static string DatabaseServer { get; set; }
        private static string DatabaseUser { get; set; }
        private static string DatabasePassword { get; set; }
        private static string DatabaseName { get; set; }

        private static string _connectionString;

        private static MySqlConnection _mySqlConnection;

        #endregion

        static MyConnection()
        {
            DatabaseServer = "";
            DatabaseUser = "";
            DatabasePassword = "";
            DatabaseName = "";
            //BuildConnction();
            //CheckOpenConnection();
        }

        /// <summary>
        /// Set the connction string and create new reference in case of null connection
        /// </summary>
        private static void BuildConnction()
        {
            _connectionString = string.Format(@"server={0};User ID={1};password={2};database={3};port=3306;old guids=true; connectiontimeout=30;",
                DatabaseServer,
                DatabaseUser,
                DatabasePassword,
                DatabaseName);
            _mySqlConnection = new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// Set database connection parameters
        /// </summary>
        /// <param name="databaseServer"></param>
        /// <param name="databaseName"></param>
        /// <param name="databaseUser"></param>
        /// <param name="databasePassword"></param>
        public static void SetDatabaseParameters(string databaseServer, string databaseName, string databaseUser, string databasePassword)
        {
            DatabaseServer = databaseServer;
            DatabaseUser = databaseUser;
            DatabasePassword = databasePassword;
            DatabaseName = databaseName;
            BuildConnction();
            CheckOpenConnection();
        }

        public static void CloseConnection()
        {
            try
            {
                _mySqlConnection.Close();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Check net connection access
        /// </summary>
        /// <returns>Return true if there is net access and false otherwise</returns>
        private static ExcuteStateMessage CheckNetConnection()
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send("8.8.8.8");
                if (reply.Status == IPStatus.Success)
                {
                    return new ExcuteStateMessage(true);
                }
                else
                {
                    return new ExcuteStateMessage(false, "NetConnection Error: there is no connection to the internet please check your net connection");
                }
            }
            catch (Exception ex)
            {
                return new ExcuteStateMessage(false, "NetConnection Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Check if the connection is null or closed and build it if null
        /// </summary>
        /// <param name="mySqlConnection"></param>
        /// <returns>Return false if the connection is null or closed and true otherwise</returns>
        private static ExcuteStateMessage CheckDatabaseConnection(MySqlConnection mySqlConnection)
        {
            if (mySqlConnection == null)
            {
                BuildConnction();
                return new ExcuteStateMessage(false, "Connection Error: null connection");
            }

            if (mySqlConnection.State != ConnectionState.Open)
            {
                return new ExcuteStateMessage(false, "Connection Error: connection is closed");
            }
            else
            {
                return new ExcuteStateMessage(true, "Connection has been opened successfully");
            }
        }

        /// <summary>
        /// Check and try to open the connection if close
        /// </summary>
        /// <returns>Return ExcuteStateMessage with true status if the connection is opened successfully 
        /// or false status on error with error message</returns>
        public static ExcuteStateMessage CheckOpenConnection()
        {
            try
            {
                // Return if the connection is open
                ExcuteStateMessage checkDBConnState = CheckDatabaseConnection(_mySqlConnection);
                if (checkDBConnState.Status)
                {
                    return new ExcuteStateMessage(true);
                }

                // Return if no net connection
                ExcuteStateMessage netConnectionState = CheckNetConnection();
                if (!netConnectionState.Status)
                {
                    return netConnectionState;
                }
                _mySqlConnection.Open();
                // Adjest time zone due to server differnet timezone
                MySqlCommand sqlCommand = new MySqlCommand("SET time_zone = '+2:00'; SET SQL_SAFE_UPDATES = 1", _mySqlConnection);
                sqlCommand.ExecuteNonQuery();
                return new ExcuteStateMessage(true);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("** " + ex.Message);
                if (connectionAttempNumber <= CONNECTION_ATTEMPS_MAX)
                {
                    connectionAttempNumber++;
                    System.Threading.Thread.Sleep(100);
                    ExcuteStateMessage excuteState = CheckOpenConnection();
                    return excuteState;
                }
                else
                {
                    connectionAttempNumber = 0;
                    return new ExcuteStateMessage(false, "Connection Exception: " + ex.Message);
                }
            }
        }

        public static MySqlConnection Connection
        {
            get { return _mySqlConnection; }
        }

        public static ConnectionState MyConnectionState
        {
            get { return Connection.State; }
        }

        internal static MySqlConnection NewConnectionInstance
        {
            get { return new MySqlConnection(_connectionString); }
        }

        /// <summary>
        /// Set this connection to point to another connection
        /// </summary>
        /// <param name="connection"></param>
        public static void SetConnection(MySqlConnection connection)
        {
            _mySqlConnection = connection;
        }
    }
}
