using System;
using System.IO;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace WeaponThread
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, int.MaxValue)]
    public class Session : MySessionComponentBase
    {
        internal WeaponStructure.WeaponDefinition[] WeaponDefinitions;

        public override void LoadData()
        {
            Log.Init($"{ModContext.ModName}Init.log");
            MyAPIGateway.Utilities.RegisterMessageHandler(7772, Handler);
            Init();
            SendModMessage(true);
        }

        protected override void UnloadData()
        {
            Log.Close();
            MyAPIGateway.Utilities.UnregisterMessageHandler(7772, Handler);
            Array.Clear(Storage, 0, Storage.Length);
            Storage = null;
        }

        void Handler(object o)
        {
            if (o == null) SendModMessage(false);
        }

        void SendModMessage(bool sending)
        {
            if (sending) Log.CleanLine($"Sending request to core");
            else Log.CleanLine($"Receiving request from core");
            MyAPIGateway.Utilities.SendModMessage(7771, Storage);
            MyAPIGateway.Utilities.SendModMessage(7773, ArmorStorage);
        }

        internal byte[] Storage;
        internal byte[] ArmorStorage;

        internal void Init()
        {
            var weapons = new Weapons();
            WeaponDefinitions = weapons.ReturnDefs();
            Log.CleanLine($"Found: {WeaponDefinitions.Length} weapon definitions");
            for (int i = 0; i < WeaponDefinitions.Length; i++)
            {
                Log.CleanLine($"Compiling: {WeaponDefinitions[i].HardPoint.WeaponName}");
                WeaponDefinitions[i].ModPath = ModContext.ModPath;

            }
            var ArmorDefinitions = weapons.ReturnArmorDefs();
            Log.CleanLine($"Found: {ArmorDefinitions.Length} armor compatibility definitions");
            Storage = MyAPIGateway.Utilities.SerializeToBinary(WeaponDefinitions);
            ArmorStorage = MyAPIGateway.Utilities.SerializeToBinary(ArmorDefinitions);
            Array.Clear(WeaponDefinitions, 0, WeaponDefinitions.Length);
            WeaponDefinitions = null;
            Log.CleanLine($"Handing over control to Core and going to sleep");
        }
    }
    public class Log
    {
        private static Log _instance = null;
        private TextWriter _file = null;

        private static Log GetInstance()
        {
            return _instance ?? (_instance = new Log());
        }

        public static void Init(string name)
        {
            if (GetInstance()._file == null)
                GetInstance()._file = MyAPIGateway.Utilities.WriteFileInLocalStorage(name, typeof(Log));
        }

        public static void CleanLine(string text)
        {
            if (GetInstance()._file == null) return;
            var message = $"{DateTime.Now:MM-dd-yy_HH-mm-ss-fff} - " + text;
            GetInstance()._file.WriteLine(message);
            GetInstance()._file.Flush();
        }

        public static void Close()
        {
            var instance = (GetInstance());
            if (instance._file == null) return;
            instance._file.Flush();
            instance._file.Close();
            instance._file.Dispose();
            instance._file = null;
            instance = null;
        }
    }

}

