"""Create metadata only for new Catventure files; never change an existing GUID."""
import uuid
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
paths=[ROOT/'Assets/_Game/Resources/Content/CatRescue.json']
paths+=list((ROOT/'Assets/_Game/Scripts').rglob('*Cat*.cs'))
paths+=[ROOT/'Assets/_Game/Resources/CatRescue',ROOT/'Assets/_Game/Resources/CatRescue/RescueCats.png',ROOT/'Assets/_Game/Resources/CatRescue/DaLatStation.png']
for path in paths:
    meta=Path(str(path)+'.meta')
    if meta.exists():continue
    guid=uuid.uuid5(uuid.NAMESPACE_URL,'catventure-20261107/'+path.relative_to(ROOT).as_posix()).hex
    text='fileFormatVersion: 2\nguid: '+guid+'\n'
    if path.is_dir():text+='folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    elif path.suffix=='.png':
        sprite=path.stem=='DaLatStation'
        text+='''TextureImporter:
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  spriteMode: SPRITE_MODE
  spriteExtrude: 1
  spriteMeshType: 1
  spriteAlignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: TEXTURE_TYPE
  textureShape: 1
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: SPRITE_ID
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
'''.replace('SPRITE_MODE','1' if sprite else '0').replace('TEXTURE_TYPE','8' if sprite else '0').replace('SPRITE_ID',uuid.uuid5(uuid.NAMESPACE_URL,guid+'/sprite').hex)
    meta.write_text(text,encoding='utf-8')
    print('Created',meta.relative_to(ROOT))
