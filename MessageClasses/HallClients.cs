using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossChessServer.MessageClasses
{
    internal class HallClients : BaseMessage
    {
        public int[] clientIds;
        public string[] clientNames;
        public bool[] clientIdleStates;

        public HallClients()
        {

        }

        public HallClients(Dictionary<int, UserInfo> hallClientDict)
        {
            // 将字典的键和值分别转换为数组
            this.clientIds = hallClientDict.Keys.ToArray();
            this.clientNames = new string[clientIds.Length];
            this.clientIdleStates = new bool[clientIds.Length];

            // 填充用户名和闲忙状态
            for (int i = 0; i < clientIds.Length; i++)
            {
                int clientId = clientIds[i];
                UserInfo userInfo = hallClientDict[clientId];
                this.clientNames[i] = userInfo.Name;
                this.clientIdleStates[i] = userInfo.IsIdle;
            }
        }

        public override byte[] ConvertToByteArray()
        {
            int index = 0;
            byte[] bytes = new byte[sizeof(int) + GetBytesNum()];
            WriteInt(bytes, (int)GetMessageID(), ref index);
            WriteIntList(bytes, clientIds, ref index);
            WriteStringList(bytes, clientNames, ref index);
            WriteBoolList(bytes, clientIdleStates, ref index);

            return bytes;
        }

        public override int GetBytesNum()
        {
            int size = 3 * sizeof(int);
            for (int i = 0; i < clientIds.Length; i++)
            {
                size += sizeof(int); // clientIds[i]
                size += sizeof(int) + Encoding.UTF8.GetBytes(clientNames[i]).Length; // clientNames[i]
                size += sizeof(bool); // clientIdleStates[i]
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
            clientIdleStates = ReadBoolList(bytes, ref index);

            return index- beginIndex;
        }
    }
}
