using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonderland_Private_Server.Code.Objects;
using Wonderland_Private_Server.Code.Enums;

namespace Wonderland_Private_Server.Bots
{
    public abstract class GmBot : Character
    {
        public GmBot()
            : base()
        {
            ID = 100;
            Body = BodyStyle.none;
            Element = (ElementType)1;
            LoginMap = 10019;
            X = 722;
            Y = 995;
            Head = 0;
            ID = 100;
            HairColor = 44828;
            SkinColor = 6781;
            ClothingColor = 44828;
            EyeColor = 6781;
            Job = RebornJob.none;
            Nickname = "";
        }
    }
}
