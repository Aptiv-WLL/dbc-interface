using System.Collections.Generic;

namespace Aptiv.DBCFiles
{
    /// <summary>
    /// An ECU object described by a DBC file.
    /// </summary>
    public interface IDBCECU
    {
        /// <summary>
        /// The name of this ECU.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The set of messages in this ECU.
        /// </summary>
        IReadOnlyDictionary<string, IDBCMessage> Messages { get; }

        /// <summary>
        /// The set of messages in this ECU marked as being RX.
        /// </summary>
        IReadOnlyDictionary<string, IDBCMessage> RxMessages { get; }
    }
}
