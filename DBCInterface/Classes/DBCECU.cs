using System;
using System.Collections.Generic;

namespace Aptiv.DBCFiles.Classes
{
    [Serializable]
    internal class DBCECU : IDBCECU
    {
        /// <summary>
        /// The name of this particular ECU.
        /// </summary>
        public string Name;

        /// <summary>
        /// Key = message name; Value = DBCMessage
        /// </summary>
        [NonSerialized]
        public Dictionary<string, IDBCMessage> Messages = new Dictionary<string, IDBCMessage>();

        /// <summary>
        /// Dictionary of messages (and their signals) that this ECU is 
        /// marked as receiving.
        /// </summary>
        [NonSerialized]
        public Dictionary<string, IDBCMessage> RxMessages = new Dictionary<string, IDBCMessage>();

        /// <summary>
        /// Construct a new ECU.
        /// </summary>
        /// <param name="ecu_name"></param>
        public DBCECU(string ecu_name)
        {
            Name = ecu_name;
        }

        /// <summary>
        /// Convert an instance of a ECU to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        #region --- IECU Interface ---

        string IDBCECU.Name => Name;

        IReadOnlyDictionary<string, IDBCMessage> IDBCECU.Messages => Messages;

        IReadOnlyDictionary<string, IDBCMessage> IDBCECU.RxMessages => RxMessages;

        #endregion
    }
}
