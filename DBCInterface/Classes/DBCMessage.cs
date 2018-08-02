using Aptiv.Messages.CAN;
using System;
using System.Collections.Generic;

namespace Aptiv.DBCFiles.Classes
{
    /// <summary>
    /// The message objects constructed by a DBC file.
    /// </summary>
    [Serializable]
    internal class DBCMessage : CANMessage, IDBCMessage
    {
        #region --- Parameters ---

        public Dictionary<string, IDBCSignal> Signals = new Dictionary<string, IDBCSignal>();

        /// <summary>
        /// Defines the way to send.
        /// </summary>
        public string LaunchType;

        /// <summary>
        /// The time to wait before sending this message.
        /// </summary>
        public int DelayTime = 0;

        /// <summary>
        /// The ECU that groups messages.
        /// </summary>
        public IDBCECU ECU;

        /// <summary>
        /// The name of this message.
        /// </summary>
        public string Name;

        /// <summary>
        /// True if message is sent on a time interval.
        /// </summary>
        public bool Cyclic { get => CycleTime != 0; }

        /// <summary>
        /// The cycle time in milliseconds if the message is cyclic.
        /// </summary>
        public uint CycleTime = 0;

        /// <summary>
        /// Event handler for handling updates to the DBC Messages
        /// </summary>
        /// <example>
        /// 
        ///     //The following would be the custom function that contains all functionallity 
        ///     //that should be executed on changes to the DBCMessage.
        ///     //example code:
        ///     //e is the default custom DBCMessageEventArgs, 
        ///         //contains the current DBCMessage and all related DBCSignals.
        ///         //This does not need to be changed or called. 
        ///         //The handler will be called automatically on changes to any 
        ///         //DBCMessage that is changed.
        ///     //
        /// 
        /// public void DBCFunctionToBeDoneOnUpdate(Object sender, DBCMessageEventArgs e)
        /// {
        ///     //Do any functionality needed on updates
        /// }
        /// 
        ///     //On DBCMessage creation, add an event handler that will 
        ///     //activate on any change to the Message.
        ///     //If using a custom function, add it in the following format:
        ///     //newMessage.DBCUpdate += myFunctionName;
        ///     //example code:
        /// 
        /// newDBCMessage.DBCUpdate += DBCChanged;
        /// 
        /// </example>
        public event EventHandler<DBCMessageUpdateEventArgs> DBCUpdate;

        /// <summary>
        /// Private storage for the string representation of the message id
        /// string representation.
        /// </summary>
        private string mid_s;

        #endregion

        #region --- IDBCMessage Interface ---

        IReadOnlyDictionary<string, IDBCSignal> IDBCMessage.Signals 
            => Signals;

        string IDBCMessage.Name => Name;

        uint IDBCMessage.CycleTime => CycleTime;

        IDBCECU IDBCMessage.ECU => ECU;

        string IDBCMessage.LaunchType => LaunchType;

        int IDBCMessage.DelayTime => DelayTime;

        /// <summary>
        /// Updates this dbc message with the data from another CANMessage.
        /// If the message id of the CAN message does not match this dbc
        /// message's id or if the length of the CAN message's Data does
        /// not match this message's Data lenght then it will not copy the
        /// data and will return false.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        bool IDBCMessage.Update(CANMessage m)
        {
            if (Message_ID != m.Message_ID) return false;
            if (Data.Count != m.Data.Count) return false;

            // Copy over the data from the can message to the data of this message.
            for (int i = 0; i < Data.Count && i < m.Data.Count; i++)
                ((IList<byte>)Data)[i] = ((IList<byte>)m.Data)[i];

            return true;
        }

        #endregion

        #region --- Constructors ---

        /// <summary>
        /// Create a new DBC Message from information provided in a .dbc file.
        /// </summary>
        public DBCMessage(int byteCount, IDBCECU ecu, string MessageID_s, string MessageName)
            : base(byteCount)
        {
            // Assign static referenced values first.
            ECU = ecu;
            Name = MessageName;
            mid_s = MessageID_s;

            // Init the internal CAN Message
            byte[] mid = StringToBytes(MessageID_s);

            // Set the message id bytes
            if (mid.Length > 4) throw new ArgumentException("The parsed message id is too long.", MessageID_s);
            else if (mid.Length == 0) throw new ArgumentException("The parsed message id is of length 0.", MessageID_s);
            else if (mid.Length % 2 == 0)
            {
                // all bytes as expected
                for (int i = 0; i < mid.Length; i++)
                    this[i] = mid[i];
            }
            else
            {
                // 0x00 is first byte
                for (int i = 0; i < mid.Length; i++)
                    this[i + 1] = mid[i];
            }
        }

        /// <summary>
        /// Constructs a new DBCMessage from another instantiated one.
        /// </summary>
        /// <param name="other">The DBCMessage to copy.</param>
        public DBCMessage(DBCMessage other) : base(other)
        {
            // Assign static referenced values first
            ECU = other.ECU;
            Name = other.Name;
            mid_s = other.mid_s;

            // Deep copy Signals
            foreach (var sig in other.Signals)
                Signals.Add(sig.Key, new DBCSignal((DBCSignal)sig.Value));

            // Other attributes
            LaunchType = other.LaunchType;
            DelayTime = other.DelayTime;
            CycleTime = other.CycleTime;
        }

        /// <summary>
        /// Clone this object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            // Make sure that the mid string is set.
            if (string.IsNullOrEmpty(mid_s))
                mid_s = Message_ID.ToString();

            var nm = new DBCMessage(Count, ECU, mid_s, Name);

            // Signals
            foreach (IDBCSignal sig in Signals.Values)
                nm.Signals.Add(sig.Name, sig);

            return nm;
        }

        #endregion

        #region --- Helpers ---

        /// <summary>
        /// Converts a string of bytes to an array of bytes.
        /// </summary>
        /// <param name="data">The string to convert.</param>
        /// <returns>An array of bytes half the length of the string.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        public static byte[] StringToBytes(string data)
        {
            if (data == null)
                throw new ArgumentNullException();

            // remove indicators and spaces
            data = data.Replace("h", "").Replace(" ", "").Replace("0x", "");

            if (data.Length % 2 != 0)
                data = "0" + data;

            byte[] arr = new byte[data.Length >> 1];

            for (int i = 0; i < (data.Length >> 1); ++i)
            {
                arr[i] = (byte)((GetHexVal(data[i << 1]) << 4) + (GetHexVal(data[(i << 1) + 1])));
            }

            return arr;
        }

        /// <summary>
        /// Converts a byte array to a string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string BytesToString(byte[] data)
        {
            if (data == null || data.Length == 0) return "";

            string res = "0x";
            res += BitConverter.ToString(data).Replace("-", "");
            return res;
        }

        /// <summary>
        /// Obtains the hex value of a valid hex character.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private static int GetHexVal(char hex)
        {
            int val = hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        #endregion

        #region --- Updating ---

        /// <summary>
        /// Will attempt to invoke the event handler.
        /// </summary>
        /// <param name="e"></param>
        public void OnDBCUpdate(DBCMessageUpdateEventArgs e)
        {
            DBCUpdate?.Invoke(this, e);
        }

        /// <summary>
        /// Updates this DBCMessage from a received CANMessage.
        /// </summary>
        /// <param name="received">The message received.</param>
        public void UpdateFromReceived(CANMessage received)
        {
            if (!(Message_ID.Equals(received.Message_ID))) return;
            else
            {
                // Create a deep clone of this message
                var copy = new DBCMessage(this);

                // Copy over the new message's data.
                for (int i = 0; i < received.Data.Count && i < copy.Data.Count; i++)
                    ((IList<byte>)copy.Data)[i] = ((IList<byte>)received.Data)[i];

                IList<string> signalsList = new List<string>();

                // Update this message's signals with the copy's
                // new signal values.
                foreach (var sig in Signals)
                {
                    // Check for modified signals and store them
                    if (Signals[sig.Key].Value != copy.Signals[sig.Key].Value)
                    {
                        signalsList.Add(sig.Key);
                    }
                    Signals[sig.Key].Value = copy.Signals[sig.Key].Value;
                }

                // Call the onUpdate function that will call the handler
                OnDBCUpdate(new DBCMessageUpdateEventArgs(signalsList));
            }
        }

        #endregion
    }
}
