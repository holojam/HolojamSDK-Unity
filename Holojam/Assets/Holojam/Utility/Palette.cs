//Palette.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;

namespace Holojam.Utility{
   public class Palette{
      public enum Colors{
         BLUE,GREEN,RED,YELLOW,SEA_FOAM
      };
      public static Color Select(Colors color){
         switch(color){
            case Colors.BLUE:
               return new Color(0,.5f,1);
            case Colors.GREEN:
               return new Color(.45f,1,.45f);
            case Colors.RED:
               return new Color(1,0,.38f);
            case Colors.YELLOW:
               return new Color(1,.84f,0);
            case Colors.SEA_FOAM:
               return new Color(.26f,.86f,.6f);
            default:
               return Color.white;
         }
      }
   }
}
