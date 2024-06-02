namespace PlayersWorlds.Maps.Serializer {
    public interface IStringSerializer {
        string Serialize(object obj);
        T Deserialize<T>(string str);
    }

}