using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Aptiv.DBCFiles.Classes;
using Aptiv.Messaging;
using System.Threading.Tasks;

namespace Aptiv.DBCFiles
{
    /// <summary>
    /// Contains all content of a parsed dbc file in a class structure and
    /// provides methods for parsing.
    /// </summary>
    [Serializable]
    public class DBCFile : IDBCFile
    {
        #region --- Globals ---

        private Dictionary<string, int> NS_Lib = new Dictionary<string, int>();
        private Dictionary<string, int> BS_Lib = new Dictionary<string, int>();
        private Dictionary<int, string> MsgSendTypes = new Dictionary<int, string>();
        private Dictionary<string, DBCECU> ecus = new Dictionary<string, DBCECU>();
        private Dictionary<string, DBCMessage> messages = new Dictionary<string, DBCMessage>();

        /// <summary>
        /// Specific attributes defined in the DBC file. (eg. "baudrate")
        /// </summary>
        private Dictionary<string, Dictionary<string, List<string>>> attributes = new Dictionary<string, Dictionary<string, List<string>>>();

        /// <summary>
        /// Groups of signals defined in the DBC file.
        /// </summary>
        private Dictionary<string, List<IDBCSignal>> signalGroups = new Dictionary<string, List<IDBCSignal>>();

        /// <summary>
        /// The DBC filename, with path, used to generate this instance of the DBCFile class.
        /// </summary>
        public string Filename { get; private set; }

        #region --- IDBCFile Interface ---

        /// <summary>
        /// The collection of ECUs.
        /// </summary>
        public IReadOnlyDictionary<string, IDBCECU> ECUs
            => (IReadOnlyDictionary<string, IDBCECU>)ecus;
        /// <summary>
        /// The collection of DBC Messages.
        /// </summary>
        public IReadOnlyDictionary<string, IDBCMessage> Messages
            => (IReadOnlyDictionary<string, IDBCMessage>)messages;
        /// <summary>
        /// The attributes of this dbc.
        /// </summary>
        public Dictionary<string, Dictionary<string, List<string>>> Attributes => attributes;
        /// <summary>
        /// The signal groupings of this dbc.
        /// </summary>
        public Dictionary<string, List<IDBCSignal>> SignalGroups => signalGroups;

        #endregion

        #endregion

        #region --- Constructors ---

        /// <summary>
        /// Construct a new Database object from the given dbc filename.
        /// </summary>
        /// <param name="filename">The file to parse.</param>
        /// <param name="parse">Set to false to delay parsing the file.</param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DatabaseLoadingException"></exception>"
        public DBCFile(string filename, bool parse = true)
        {
            if (File.Exists(filename))
            {
                Filename = filename;
                if (parse) CreateLibraryDBC();
            }
            else
                throw new FileNotFoundException("The given database file does not exist.", filename);
        }

        #endregion

        #region --- Parsing ---

        /// <summary>
        /// Used to create a message library from a .dbc file
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="DatabaseLoadingException"></exception>
        public void CreateLibraryDBC()
        {
            if (Filename == "") return;
            string s;
            using (StreamReader sr = new StreamReader(Filename))
                s = sr.ReadToEnd();

            Parse(s);
        }

        /// <summary>
        /// Create the library using async.
        /// </summary>
        public async Task CreateLibraryDBCAsync()
        {
            if (Filename == "") return;
            string s;
            using (StreamReader sr = new StreamReader(Filename))
                s = await sr.ReadToEndAsync();

            await Task.Run(()=> { Parse(s); });
        }

        /// <summary>
        /// Parse the given DBCFile string into the librarys.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        private void Parse(string s)
        {
            NS_Lib = new Dictionary<string, int>();
            BS_Lib = new Dictionary<string, int>();
            MsgSendTypes = new Dictionary<int, string>();
            ecus = new Dictionary<string, DBCECU>();
            messages = new Dictionary<string, DBCMessage>();
            attributes = new Dictionary<string, Dictionary<string, List<string>>>();
            signalGroups = new Dictionary<string, List<IDBCSignal>>();

            #region --- Strings for matches ---
            string NS = @"NS_[\t ]*:[\r\t ]*\n(\t[\w]+[\r\t ]*\n)*";
            string BU = @"BU_\s *:(\s +.*)*";
            string BOline = @"BO_\s+\d+\s+\w+:\s*\d+\s+\w+";
            string SGline = @"SG_\s+\w+\s*\w*\s*:\s*\d+\|\d+@[01][+-]\s+\([\d.-]*,[\d.-]*\)\s+\[.*\|.*\]\s+"".*""(\s+\w+,*\s*)+";
            string BO = BOline + @"\s+(" + SGline + @")*";
            string CM = @"CM_\s+\w+\s+\w+(\s+\w+)*\s+""([^""]*)*""";
            string BA_DEF = @"BA_DEF_\s+.*;";
            string BA_DEF_DEF = @"BA_DEF_DEF_\s+.*;";
            string BA = @"BA_\s+.*;";
            string VAL = @"VAL_\s+.*;";
            //string SIG_GROUP = @"";
            #endregion

            // init some values
            string[] tmp = new string[0];
            int i = -1, DataLength = 0;
            string ECU_Name = "", Msg_Name = "", Snl_name = "", MsgIDHex = "";
            string AttributeName = "", AttributeValue = "";
            try
            {
                #region --- New Symbols ---
                foreach (Match m in Regex.Matches(s, NS))
                {
                    // Remove NS_ and split by whitespace
                    string[] keys = m.Value.Split('\n');

                    // Add to NS lib if not already there
                    for (int ki = 1; ki < keys.Length; ki++)
                        if (!NS_Lib.ContainsKey(keys[ki]) && !string.IsNullOrEmpty(keys[ki]))
                            NS_Lib.Add(keys[ki], NS_Lib.Count);
                }
                #endregion

                #region --- ECUs ---
                foreach (Match m in Regex.Matches(s, BU))
                {
                    // Remove BU_ and split by whitespace
                    tmp = Regex.Split(m.Value.Replace("BU_", ""), @"\s+");

                    // Add to ECU libs
                    foreach (string str in tmp)
                        if (!ecus.ContainsKey(str))
                        {
                            //ECU_Lib.Add(str, ECU_Lib.Count);
                            ecus.Add(str, new DBCECU(tmp[i]));
                        }
                }
                #endregion

                #region --- Messages & Signals ---
                foreach (Match m in Regex.Matches(s, BO))
                {
                    // Deal with Message line: BO_ Tag | Message ID | Message Name | Message Length (bytes) | ECU
                    string messageLine = Regex.Match(m.Value, BOline).Value;

                    //split by whitespace
                    tmp = Regex.Split(messageLine.Replace(":", ""), @"\s+");

                    // Message Attributes
                    ECU_Name = tmp[4];
                    Msg_Name = tmp[2];
                    int midLen = tmp[1].Length > 4 ? 4 : 2;
                    MsgIDHex = "0x" + Convert.ToInt64(tmp[1]).ToString("X3");
                    DataLength = Convert.ToInt32(tmp[3]);


                    //If ECU doesn't exist, create it
                    if (!ecus.ContainsKey(ECU_Name))
                        ecus.Add(ECU_Name, new DBCECU(ECU_Name));

                    // Add a new message
                    var newMessage = new DBCMessage(midLen + DataLength, ecus[ECU_Name], MsgIDHex, Msg_Name);
                    messages.Add(MsgIDHex, newMessage);
                    
                    // Add message to Messages dictionary in current ECU
                    if(!ecus[ECU_Name].Messages.ContainsKey(MsgIDHex)) {
                        ecus[ECU_Name].Messages[MsgIDHex] = (IDBCMessage)newMessage; 
                    }
                    
                    // Deal with signal line:
                    // SG_ Tag | Signal ID | Optional Multiplexer | Start Bit + Length (bits) + Byte Direction | (Factor, Offset) | Physical Range [min,max] | Physical Units | Response ECU
                    foreach (Match sig in Regex.Matches(m.Value, SGline))
                    {
                        int multiplexer = 0; // 1 if there is a multiplexer, 0 if not
                        if (Regex.IsMatch(sig.Value, @"\w+\s+\w+\s+\w+\s*:"))
                            multiplexer = 1;

                        tmp = Regex.Split(Regex.Replace(sig.Value, @"[\(\)\[\]\|""@,:]", " "), @"\s+");

                        Snl_name = tmp[1];

                        // Signal Attributes
                        int startBit = Convert.ToInt32(tmp[2 + multiplexer]);
                        int length = Convert.ToInt32(tmp[3 + multiplexer]);
                        IntDescriptor descriptor = (tmp[4 + multiplexer][1] == '+') ? IntDescriptor.Unsigned : IntDescriptor.Signed;
                        ByteOrder byteOrder;
                        if (tmp[4 + multiplexer][0] == '1')
                            byteOrder = ByteOrder.Intel;
                        else byteOrder = ByteOrder.Motorola;
                        double factor = Convert.ToDouble(tmp[5 + multiplexer], System.Globalization.CultureInfo.InvariantCulture);
                        double offset = Convert.ToDouble(tmp[6 + multiplexer], System.Globalization.CultureInfo.InvariantCulture);
                        double physMin = Convert.ToDouble(tmp[7 + multiplexer], System.Globalization.CultureInfo.InvariantCulture);
                        double physMax = Convert.ToDouble(tmp[8 + multiplexer], System.Globalization.CultureInfo.InvariantCulture);
                        string physUnits = tmp[9 + multiplexer];

                        //If physMin and physMax are both 0, then physMax is as high as the largest bitvalue
                        physMax = (physMin == 0 && physMax == 0) ? (1 << length) : physMax;

                        // These values should be 0 and (2 << length - 1)
                        ulong normMin = (ulong)((physMin - offset) / factor);
                        ulong normMax = (ulong)((physMax - offset) / factor);
                        double res = (physMax - physMin) / (normMax - normMin);
                        if (double.IsInfinity(res) || double.IsNaN(res))
                            res = 1;

                        // Responce ECUs
                        for (i = 10 + multiplexer; i < tmp.Length; i++)
                        {
                            if (!ecus.ContainsKey(tmp[i]))
                                ecus.Add(tmp[i], new DBCECU(tmp[i]));
                            if (!ecus[tmp[i]].RxMessages.ContainsKey(MsgIDHex))
                                ecus[tmp[i]].RxMessages.Add(MsgIDHex, messages[MsgIDHex]);
                        }

                        int old = startBit;

                        // TODO: verify this is needed.
                        // account for motorola being inverse
                        if (byteOrder == ByteOrder.Motorola)
                        {
                            int newStartBit = startBit;
                            for (i = 1; i < length; i++)
                            {
                                if (newStartBit % 8 == 0)
                                    newStartBit += 15;
                                else
                                    newStartBit -= 1;
                            }
                            startBit = newStartBit;
                        }

                        // Add a new signal
                        messages[MsgIDHex].Signals.Add(Snl_name, new DBCSignal(ecus[ECU_Name],
                            messages[MsgIDHex], (uint)startBit, Snl_name, (uint)length, 0, 0,
                            physMax, physMin, normMax, normMin, res, physUnits, descriptor,
                            byteOrder, offset, sig.Value));
                    }
                }




                #endregion

                #region --- Functionality Description (comments) ---
                foreach (Match m in Regex.Matches(s, CM))
                {
                    /*  Note:
                     *  Only existing signals can have comments parsed for them,
                     *  there is no way to generate a new signal or message from
                     *  a comment statement. */

                    if (Regex.IsMatch(m.Value, @"SG_"))
                    {
                        tmp = Regex.Split(m.Value, @"\s+");
                        MsgIDHex = "0x" + Convert.ToInt64(tmp[2]).ToString("X3");
                        if (messages.ContainsKey(MsgIDHex))
                        {
                            Snl_name = tmp[3];
                            if (messages[MsgIDHex].Signals.ContainsKey(Snl_name))
                            {
                                string function = "";
                                for (i = 4; i < tmp.Length; i++)
                                    function += (tmp[i] + " ").Replace("\"", "");
                                ((DBCSignal)messages[MsgIDHex].Signals[Snl_name]).Function = function;
                            }
                        }
                    }
                }
                #endregion

                #region --- Base Attribute Definitions ---
                foreach (Match m in Regex.Matches(s, BA_DEF))
                {
                    string attributeName;
                    string attributeType;
                    string tag = "Definition";
                    List<string> attributeSettings = new List<string>();
                    tmp = Regex.Split(m.Value, @"\s+");

                    if (Regex.IsMatch(m.Value, @"BU_"))
                    {
                        attributeName = tmp[2].Replace("\"", "");
                        attributeType = tmp[3];
                        for (int j = 4; j < tmp.Length; j++)
                            attributeSettings.Add(tmp[j].Replace(";", ""));
                    }
                    else if (Regex.IsMatch(m.Value, @"BO_"))
                    {
                        attributeName = tmp[2].Replace("\"", "");
                        attributeType = tmp[3];
                        for (int j = 4; j < tmp.Length; j++)
                            attributeSettings.Add(tmp[j].Replace(";", ""));

                        // OG code... fix in future
                        switch (attributeName)
                        {
                            case "GenMsgSendType":  //Message send type(cyclic, spontaneous, etc..)
                                string[] values = tmp[tmp.Length - 1].Split(',');
                                for (int k = 0; k < values.Length; k++)
                                {
                                    //k is Enum int value, values[k] is string it represents
                                    MsgSendTypes.Add(k, values[k].Replace("\"", "").Replace(";", ""));
                                }
                                break;
                            case "GenMsgCycleTime":
                                break;
                            case "GenMsgStartDelayTime":
                                break;
                            case "GenMsgDelayTime":
                                break;
                            default:
                                break;
                        }
                    }
                    else if (Regex.IsMatch(m.Value, @"SG_"))
                    {
                        attributeName = tmp[2].Replace("\"", "");
                        attributeType = tmp[3];
                        for (int j = 4; j < tmp.Length; j++)
                            attributeSettings.Add(tmp[j].Replace(";", ""));
                    }
                    else
                    {
                        // extract info
                        attributeName = tmp[1].Replace("\"", "");
                        attributeType = tmp[2];
                        for (int j = 3; j < tmp.Length; j++)
                            attributeSettings.Add(tmp[j].Replace(";", ""));

                        // add to dict
                        if (attributes.ContainsKey(attributeName))
                        {
                            if (attributes[attributeName].ContainsKey(tag))
                                attributes[attributeName][tag].Concat(attributeSettings);
                            else
                                attributes[attributeName].Add(tag, attributeSettings);
                        }
                        else
                        {
                            attributes.Add(attributeName, new Dictionary<string, List<string>>());
                            attributes[attributeName].Add(tag, attributeSettings);
                        }
                    }

                }
                #endregion

                #region --- Base Attribute Definitions Defaults ---
                foreach (Match m in Regex.Matches(s, BA_DEF_DEF))
                {
                    string tag = "Default";
                    tmp = Regex.Split(Regex.Replace(m.Value, @"[;""]", ""), @"\s+");
                    if (tmp.Length < 3)
                        continue;
                    string attr = tmp[1];
                    string val = tmp[2];

                    if (attributes.ContainsKey(attr))
                    {
                        if (attributes[attr].ContainsKey(tag))
                            attributes[attr][tag].Add(val);
                        else
                            attributes[attr].Add(tag, new List<string>() { val });
                    }
                    else
                        attributes.Add(attr, new Dictionary<string, List<string>>() { { tag, new List<string>() { val } } });
                }
                #endregion

                #region --- Base Attributes ---
                foreach (Match m in Regex.Matches(s, BA))
                {
                    string tag = "Setting";
                    string attributeName;
                    string attributeValue;
                    List<string> attributeSettings = new List<string>();

                    //modified regex check to ignore spaces inside of quotes
                    tmp = Regex.Split(m.Value, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                    if (Regex.IsMatch(tmp[2], @"BO_"))
                    {
                        MsgIDHex = "0x" + Convert.ToInt64(tmp[3]).ToString("X3");

                        //Handle attributes if MsgID exists
                        if (messages.ContainsKey(MsgIDHex))
                        {
                            attributeName = tmp[1].Replace("\"", "");
                            attributeValue = tmp[4].Replace(";", "");
                            switch (attributeName)
                            {
                                case "GenMsgCycleTime": //Cycle time
                                    messages[MsgIDHex].CycleTime = uint.Parse(attributeValue);
                                    break;
                                case "GenMsgSendType": //Launch type(cyclic, spontaneous, etc.)
                                    messages[MsgIDHex].LaunchType = MsgSendTypes[int.Parse(attributeValue)];
                                    break;
                                case "GenMsgDelayTime":
                                    messages[MsgIDHex].DelayTime = int.Parse(attributeValue);
                                    break;
                            }
                        }
                    }
                    else if (Regex.IsMatch(m.Value, @"SG_"))
                    {
                        MsgIDHex = "0x" + Convert.ToInt64(tmp[3]).ToString("X3");
                        Snl_name = tmp[4];

                        AttributeName = tmp[1].Replace("\"", "");
                        AttributeValue = "";
                        for (i = 5; i < tmp.Length; i++)
                            AttributeValue += (tmp[i] + " ").Replace("\"", "").Replace(";", "");

                        // Check and add
                        if (messages.ContainsKey(MsgIDHex) &&
                            messages[MsgIDHex].Signals.ContainsKey(Snl_name))
                            ((DBCSignal)messages[MsgIDHex].Signals[Snl_name]).Attributes.Add(AttributeName, AttributeValue);
                    }
                    else
                    {
                        if (attributes.ContainsKey(tmp[1]))
                        {
                            if (attributes[tmp[1]].ContainsKey(tag))
                                attributes[tmp[1]][tag].Add(tmp[2]);
                            else
                                attributes[tmp[1]].Add(tag, new List<string>() { tmp[2] });
                        }
                        else
                            attributes.Add(tmp[1], new Dictionary<string, List<string>>() { { tag, new List<string>() { tmp[2] } } });
                        //break;
                    }
                }
                #endregion

                #region --- Specified Signal Values ---

                foreach (Match m in Regex.Matches(s, VAL))
                {
                    tmp = Regex.Split(m.Value, @"\s+");
                    MsgIDHex = "0x" + Convert.ToInt64(tmp[1]).ToString("X3");
                    Snl_name = tmp[2];

                    foreach (Match val in Regex.Matches(m.Value, "\\d+\\s+\".*?\""))
                    {
                        tmp = Regex.Split(val.Value.Replace("\"", " "), @"\s+");

                        long key = long.Parse(tmp[0]);
                        tmp[0] = "";
                        string specifiedValue = string.Join<string>(" ", tmp).Trim();

                        // Check and add
                        if (messages.ContainsKey(MsgIDHex) &&
                            messages[MsgIDHex].Signals.ContainsKey(Snl_name))
                        {
                            if (messages[MsgIDHex].Signals[Snl_name].SpecifiedValues.ContainsKey(key))
                                ((DBCSignal)messages[MsgIDHex].Signals[Snl_name]).SpecifiedValues[key] = specifiedValue;
                            else
                                ((DBCSignal)messages[MsgIDHex].Signals[Snl_name]).SpecifiedValues.Add(key, specifiedValue);
                        }
                    }
                }

                #endregion
            }
            catch (DatabaseLoadingException)
            {
                throw;
            }
            /*catch (Exception e)
            {
                throw new DatabaseLoadingException("Parsing of the given DBC file failed.", e, this, "");
            }*/

            #region old code section for Signal Groups
            /* 
                OG SIG_GROUP_ parsing below

                        #region SIG_GROUP_ (Signal Groupings)
                        case "SIG_GROUP_":

                            // Remove the tag and use space as a delimiter
                            tmp = AcquireValues(str);

                            MsgIDHex = "0x" + Convert.ToInt64(tmp[1]).ToString("X3");
                            ECU_name = MsgID_to_ECUname[MsgIDHex];
                            string Group_name = tmp[2];

                            for (int itm = 5; itm < tmp.Length; itm++)
                            {
                                Snl_name = tmp[itm];
                                if (SignalGroups.ContainsKey(Group_name))
                                    SignalGroups[Group_name].Add(Messages[MsgIDHex].Signals[Snl_name]);
                                else
                                    SignalGroups.Add(Group_name, new List<IDBCSignal>() { Messages[MsgIDHex].Signals[Snl_name] });
                            }

                            break;
                        #endregion
            } 
            */
            #endregion
        }

        /// <summary>
        /// Obtains a partial database in the case that the parsing fails
        /// by throwing a DatabaseLoadingException during parsing.
        /// </summary>
        /// <param name="filename">The file to parse.</param>
        /// <param name="partial">An argument that will be assigned false if the file is parsed completely.</param>
        /// <returns>A complete or partial database.</returns>
        public static DBCFile TryParseDatabase(string filename, out bool partial)
        {
            DBCFile file;
            try
            {
                file = new DBCFile(filename);
                partial = false;
            }
            catch (DatabaseLoadingException e)
            {
                if (e.Data.Contains("Partial Database"))
                {
                    file = (DBCFile)e.Data["Partial Database"];
                    partial = true;
                }
                else
                {
                    file = null;
                    partial = false;
                }
            }
            return file;
        }

        /// <summary>
        /// Obtains a partial database in the case that the parsing fails
        /// by throwing a DatabaseLoadingException during parsing.
        /// </summary>
        /// <param name="filename">The file to parse.</param>
        /// <returns>A complete or partial database.</returns>
        public static DBCFile TryParseDatabase(string filename)
        {
            return TryParseDatabase(filename, out bool error);
        }

        /// <summary>
        /// Obtains a partial database in the case that the parsing fails
        /// by throwing a DatabaseLoadingException during parsing.
        /// </summary>
        /// <param name="filename">The file to parse.</param>
        /// <returns>A complete or partial database.</returns>
        public static async Task<DBCFile> TryParseDatabaseAsync(string filename)
        {
            DBCFile file;
            try
            {
                file = new DBCFile(filename, false);
                await file.CreateLibraryDBCAsync();
            }
            catch (DatabaseLoadingException e)
            {
                if (e.Data.Contains("Partial Database"))
                {
                    file = (DBCFile)e.Data["Partial Database"];
                }
                else
                {
                    file = null;
                }
            }
            return file;
        }

        #endregion

        #region --- Modification ---

        /// <summary>
        /// Writes the DBC to a file
        /// </summary>
        /// <param name="fileName">filename to write data to</param>
        public void WriteToFile(string fileName)
        {
            //list of lines in the .dbc file
            List<string> lines = new List<string>
            {
                //new symbols
                "NS_ :"
            };
            foreach (KeyValuePair<string,int> ns in NS_Lib)
                lines.Add("\t" + ns.Key);

            //BS_
            lines.Add("BS_:");
            foreach (KeyValuePair<string, int> bs in BS_Lib)
                lines.Add("\t" + bs.Key);

            //ECUs
            string ecuLine = "BU_:";
            foreach (string ecu_name in ecus.Keys)
                ecuLine += " " + ecu_name;
            lines.Add(ecuLine);

            //messages
            foreach (KeyValuePair<string, DBCMessage> message in messages)
            {
                string boLine = "BO_ " + message.Key.ToString().Substring(2) + message.Value.Name + ": " 
                    + message.Value.Count + " " + message.Value.ECU;
                lines.Add(boLine);

                //signals
                string sigLine = "  SG_ ";
                foreach (KeyValuePair<string, IDBCSignal> sg in message.Value.Signals)
                {
                    sigLine += sg.Value.Name + " : "; //finish
                }

            }
        }
        
        #endregion
    }
}
