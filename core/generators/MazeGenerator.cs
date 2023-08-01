using System;
using System.Collections.Generic;

public abstract class MazeGenerator {
    public MazeGrid Generate(Dimensions mazeSize) => 
        Generate(MazeLayout.GenerateRandom(mazeSize));

    public abstract MazeGrid Generate(MazeLayout layout);


    public static byte[] GetRandomBytes(int count) {
        var rndBytes = new byte[count];
        var rnd = System.Security.Cryptography.RNGCryptoServiceProvider.Create();
        rnd.GetBytes(rndBytes);
        return rndBytes;
    }
}