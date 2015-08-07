using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace Server.Config
{
    public class DataBaseConfig : INotifyPropertyChanged
    {
        //return the following information

        //MySql/SQl Settings
        /// <summary>
        /// Username for authencation
        /// </summary>
        public string User;
        /// <summary>
        /// Pasword for authencation
        /// </summary>
        public string Pass;
        /// <summary>
        /// The Database we want to use
        /// </summary>
        public string DataBase;
        /// <summary>
        /// Port we are connecting to
        /// </summary>
        public int Port;
        /// <summary>
        /// IP Address of the Server
        /// </summary>
        public string ServerIP;
        /// <summary>
        /// Type of Server we are connecting to
        /// </summary>
        public RCLibrary.Core.DataBaseTypes Server_Type;

        string _TableName_Ref; public string TableName_Ref { get { return _TableName_Ref; } set { SetField(ref _TableName_Ref, value); } }
        string _Username_Ref; public string Username_Ref { get { return _Username_Ref; } set { SetField(ref _Username_Ref, value); } }
        string _Password_Ref; public string Password_Ref { get { return _Password_Ref; } set { SetField(ref _Password_Ref, value); } }
        public string _UserID_Ref; public string UserID_Ref { get { return _UserID_Ref; } set { SetField(ref _UserID_Ref, value); } }
        public string _CharacterID1_Ref; public string CharacterID1_Ref { get { return _CharacterID1_Ref; } set { SetField(ref _CharacterID1_Ref, value); } }
        public string _CharacterID2_Ref; public string CharacterID2_Ref { get { return _CharacterID2_Ref; } set { SetField(ref _CharacterID2_Ref, value); } }
        public string _IM_Ref; public string IM_Ref { get { return _IM_Ref; } set { SetField(ref _IM_Ref, value); } }
        public string _Char_Delete_Code_Ref; public string Char_Delete_Code_Ref { get { return _Char_Delete_Code_Ref; } set { SetField(ref _Char_Delete_Code_Ref, value); } }
        public int _PassVerifi; public int PassVerification { get { return _PassVerifi; } set { SetField(ref _PassVerifi, value); } }

        #region Inotify Property
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    struct DataBaseTableStruct
    {
        

    }
}
