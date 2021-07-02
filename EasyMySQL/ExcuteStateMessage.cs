using System;
using System.Collections.Generic;
using System.Linq;
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
    /// This class is use to repersent the status (sucuess or failed, excution message, the return data with specific type if has) of any database query opreation
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public class ExcuteStateMessage<Type>
    {
        const string MESSAGE = "تمت العملية بنجاح";

        /// <summary>
        /// Represent the status of the opreation (true in case of success)
        /// </summary>
        public bool Status
        {
            get;
            set;
        }

        /// <summary>
        /// Represent the return opreation success or failed excution message 
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Repersent the return data of the opreation 
        /// </summary>
        public Type Data
        {
            get;
            set;
        }

        // <summary>
        /// Has true if this opreation return a specific data
        /// </summary>
        public bool HasData
        {
            get;
            internal set;
        }

        /// <summary>
        /// Create new object with no data return
        /// </summary>
        /// <param name="status">Opreation excution status</param>
        /// <param name="message">Exctute status message</param>
        public ExcuteStateMessage(bool status, string message = MESSAGE)
        {
            this.Status = status;
            this.Message = message;
            this.Data = default;
            HasData = false;
        }

        /// <summary>
        /// Create new object with return data
        /// </summary>
        /// <param name="status">Opreation excution status</param>
        /// <param name="data">Return data</param>
        /// <param name="message">Exctute status message</param>
        public ExcuteStateMessage(bool status, Type data, string message = MESSAGE)
        {
            this.Status = status;
            this.Message = message;
            this.Data = data;
            if (data == null)
            {
                HasData = false;
            }
            else
            {
                HasData = true;
            }
        }

    }


    /// <summary>
    /// This class is use to repersent the status (sucuess or failed, excution message, the return data if has) of any database query opreation
    /// </summary>
    public class ExcuteStateMessage
    {

        const string MESSAGE = "تمت العملية بنجاح";

        /// <summary>
        /// Represent the status of the opreation (true in case of success)
        /// </summary>
        public bool Status
        {
            get;
            set;
        }

        /// <summary>
        /// Represent the return opreation success or failed excution message 
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Has true if this opreation return any kind of data
        /// </summary>
        public bool HasData
        {
            get;
            internal set;
        }

        /// <summary>
        /// Repersent the return data of the opreation if it has a return data
        /// </summary>
        public object Data
        {
            get;
            set;
        }

        /// <summary>
        /// Create new object with no data return
        /// </summary>
        /// <param name="status">Opreation excution status</param>
        /// <param name="message">Exctute status message</param>
        public ExcuteStateMessage(bool status, string message = MESSAGE)
        {
            this.Status = status;
            this.Message = message;
            HasData = false;
        }

        /// <summary>
        /// Create new object with return data
        /// </summary>
        /// <param name="status">Opreation excution status</param>
        /// <param name="data">Return data</param>
        /// <param name="message">Exctute status message</param>
        public ExcuteStateMessage(bool status, object data, string message = MESSAGE)
        {
            this.Status = status;
            this.Message = message;
            this.Data = data;
            HasData = true;
        }

    }
}

