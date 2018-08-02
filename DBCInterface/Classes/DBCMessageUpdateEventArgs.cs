using System;
using System.Collections.Generic;

namespace Aptiv.DBCFiles
{
    /// <summary>
    /// Event arguments for DBCMessages.
    /// </summary>
    [Serializable]
    public class DBCMessageUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// The coresponding modified signals to the message.
        /// </summary>
        public IList<string> ModifiedDBCSignals { get; private set; }

        /// <summary>
        /// Construct a new DBCMessageEventArgs with the given message and signal.
        /// </summary>
        /// <param name="signals">A list of all modified signals</param>
        public DBCMessageUpdateEventArgs(IList<string> signals)
        {
            ModifiedDBCSignals = signals;
        }
    }
}
