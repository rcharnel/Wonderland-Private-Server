using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.Code.Interface
{
    public class GuildMember
    {
        public Character OfflineSrc { private get; set; }
        public Character OnlineSrc { private get; set; }


        #region Guild Member Properties
        public virtual uint ID
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? 0 : OfflineSrc.ID : OnlineSrc.ID;
            }
        }
        public virtual string CharacterName
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? "" : OfflineSrc.CharacterName : OnlineSrc.CharacterName;
            }
        }
        public virtual string Nickname
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? "" : OfflineSrc.Nickname : OnlineSrc.Nickname;
            }
        }
        public virtual UInt16 HairColor
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? (ushort)0 : OfflineSrc.HairColor : OnlineSrc.HairColor;
            }
        }
        public virtual UInt16 SkinColor
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? (ushort)0 : OfflineSrc.SkinColor : OnlineSrc.SkinColor;
            }
        }
        public virtual UInt16 ClothingColor
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? (ushort)0 : OfflineSrc.ClothingColor : OnlineSrc.ClothingColor;
            }
        }
        public virtual UInt16 EyeColor
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? (ushort)0 : OfflineSrc.EyeColor : OnlineSrc.EyeColor;
            }
        }
        public virtual byte Busy
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? (byte)0 : OfflineSrc.Busy : OnlineSrc.Busy;
            }
        }
        /// <summary>
        /// a Character's level
        /// </summary>
        public byte Level
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? (byte)0 : OfflineSrc.Level : OnlineSrc.Level;
            }
        }
        /// <summary>
        /// Returns whether a player is reborn or not
        /// </summary>
        public bool Reborn
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? false : OfflineSrc.Reborn : OnlineSrc.Reborn;
            }
        }
        /// <summary>
        /// SubType of Character body Chosen
        /// </summary>
        public byte Head
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? (byte)0 : OfflineSrc.Head : OnlineSrc.Head;
            }
        }
        /// <summary>
        /// BodyType
        /// </summary>
        public BodyStyle Body
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? BodyStyle.none : OfflineSrc.Body : OnlineSrc.Body;
            }
        }
        /// <summary>
        /// Element Type
        /// </summary>
        public ElementType Element
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? ElementType.Undefined : OfflineSrc.Element : OnlineSrc.Element;
            }
        }
        /// <summary>
        /// Rebirth Job Type
        /// </summary>
        public RebornJob Job
        {
            get
            {
                return (OnlineSrc == null) ? (OfflineSrc == null) ? RebornJob.none : OfflineSrc.Job : OnlineSrc.Job;
            }
        }

        public bool can_DisBand_Guild { get; set; }
        public bool can_Invite { get; set; }
        public bool can_Modify_Rules { get; set; }
        public bool can_Modify_Rights { get; set; }
        public bool can_Modify_Badge { get; set; }

        public bool isOnline { get { return !(OnlineSrc == null); } }
        #endregion

        public GuildMember()
        { }
        public GuildMember(object src)
        {
            if (src is Player)
                OnlineSrc = src as Player;
            else if (src is Character)
                OfflineSrc = src as Character;
        }
    }
}
