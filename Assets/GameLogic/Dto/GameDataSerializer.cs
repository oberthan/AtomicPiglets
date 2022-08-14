using System.Collections.Generic;
using GameLogic;
using Newtonsoft.Json;

namespace Assets.Network
{
    public static class GameDataSerializer
    {
        public static List<IGameAction> DeserializeActionListJson(string actionListJson)
        {
            return JsonConvert.DeserializeObject<List<IGameAction>>(actionListJson, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });
        }

        public static string SerializeActionListJson(List<IGameAction> actionList)
        {
            return JsonConvert.SerializeObject(actionList, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            });
        }

        public static IGameAction DeserializeGameActionJson(string actionJson)
        {
            return JsonConvert.DeserializeObject<IGameAction>(actionJson, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });
        }

        public static string SerializeGameActionJson(IGameAction gameAction)
        {
            return JsonConvert.SerializeObject(gameAction, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            });
        }
    }
}