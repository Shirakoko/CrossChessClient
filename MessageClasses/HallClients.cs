using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossChessServer.MessageClasses
{
    internal class HallClients : BaseMessage
    {
        public int[] clientIds;
        public string[] clientNames;

        public HallClients()
        {

        }
    
        public HallClients(Dictionary<int, string> hallClientDict)
        {
            this.clientIds = hallClientDict.Keys.ToArray();
            this.clientNames = hallClientDict.Values.ToArray();
        }

        public override byte[] ConvertToByteArray()
        {
            int index = 0;
            byte[] bytes = new byte[sizeof(int) + GetBytesNum()];
            WriteInt(bytes, (int)GetMessageID(), ref index);
            WriteIntList(bytes, clientIds, ref index);
            WriteStringList(bytes, clientNames, ref index);

            return bytes;
        }

        public override int GetBytesNum()
        {
            int size = 2 * sizeof(int); // clientIds和clientNames数组长度
            for (int i = 0; i < clientIds.Length; i++)
            {
                size += sizeof(int); // clientIds[i]
                size += sizeof(int) + Encoding.UTF8.GetBytes(clientNames[i]).Length; // clientNames[i]
            }

            return size;
        }

        public override MessageID GetMessageID()
        {
            return MessageID.HallClients;
        }

        public override int ReadFromBytes(byte[] bytes, int beginIndex = 0)
        {
            int index = beginIndex;
            clientIds = ReadIntList(bytes, ref index);
            clientNames = ReadStringList(bytes, ref index);

            return index- beginIndex;
        }
    }
}
