namespace PlayersWorlds.Maps.Serializer {
    public class JsonSerializer : IStringSerializer {
        string IStringSerializer.Serialize(object obj) {
            throw new System.NotImplementedException();
        }

        T IStringSerializer.Deserialize<T>(string str) {
            throw new System.NotImplementedException();
        }
    }

}