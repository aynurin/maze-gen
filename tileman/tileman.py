import sys
import png
import os


tileWidth = int(sys.argv[1])
tileHeight = int(sys.argv[2])
sourceFilePath = os.path.abspath(sys.argv[3])
targetFilePath = os.path.splitext(
    sourceFilePath)[0] + "_f" + os.path.splitext(sourceFilePath)[1]

tileMap = png.Reader(sourceFilePath).read()
tileCols = int(tileMap[0] / tileWidth)
imgParams = tileMap[3].copy()
imgParams.pop("physical", None)
imgParams.pop("background", None)


def reversed(row):
    newRow = []
    tiles = 0
    for x in range(tileCols):
        newRow += row[x * tileWidth:x * tileWidth + tileWidth][::-1]
        tiles += 1
    return newRow


flippedTileMap = [reversed(row) for row in tileMap[2]]

print(len(flippedTileMap[0]), len(flippedTileMap))

flippedTileMapWriter = png.Writer(tileMap[0], tileMap[1], **imgParams)
targetFile = open(targetFilePath, 'wb')
flippedTileMapWriter.write(targetFile, flippedTileMap)
targetFile.close()
