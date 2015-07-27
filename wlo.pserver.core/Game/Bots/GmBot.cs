using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Code;

namespace Game.Bots
{
    public abstract class GmBot : Character
    {
        public GmBot()
            : base()
        {
            CharID = 100;
            Body = BodyStyle.none;
            Element = (Affinity)1;
            LoginMap = 10019;
            CurX = 722;
            CurY = 995;
            Head = 0;
            HairColor = 44828;
            SkinColor = 6781;
            ClothingColor = 44828;
            EyeColor = 6781;
            Job = RebornJob.none;
            NickName = "";
        }
        public override byte Level
        {
            get { return 220; }
        }
    }
}
