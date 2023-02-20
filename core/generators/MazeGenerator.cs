
public abstract class MazeGenerator {
    public abstract void Generate(MazeGrid maze);

    public static byte[] GetRandomBytes(int count) {
        var rndBytes = new byte[count];
        var rnd = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
        rnd.GetBytes(rndBytes);
        return rndBytes;
    }
}