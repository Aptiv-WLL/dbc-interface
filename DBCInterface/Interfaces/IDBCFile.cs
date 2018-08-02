using System.Collections.Generic;

namespace Aptiv.DBCFiles
{
    /// <summary>
    /// Describes a generic DBCFile object.
    /// </summary>
    public interface IDBCFile
    {
        /// <summary>
        /// The name of the internal file.
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// ECUs defined in the DBC file.
        /// </summary>
        IReadOnlyDictionary<string, IDBCECU> ECUs { get; }

        /// <summary>
        /// Messages defined in the DBC file.
        /// </summary>
        IReadOnlyDictionary<string, IDBCMessage> Messages { get; }

        /// <summary>
        /// Specific attributes defined in the DBC file. (eg. "baudrate")
        /// </summary>
        Dictionary<string, Dictionary<string, List<string>>> Attributes { get; }

        /// <summary>
        /// Groups of signals defined in the DBC file.
        /// </summary>
        Dictionary<string, List<IDBCSignal>> SignalGroups { get; }
    }
}
