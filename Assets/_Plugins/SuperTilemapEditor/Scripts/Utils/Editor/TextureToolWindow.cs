using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RailGun
{
	public class TextureToolWindow : EditorWindow
	{
		private Texture2D texture;
		private string saveFolder;
		private string saveName = "sliced";
		private Vector2 offset;
		private Vector2 size = new Vector2(10,10);
		private bool fade = true;
		private Vector2 padding = new Vector2(1,1);


		[MenuItem("RailGun/Texture Tool")]
		public static TextureToolWindow Init()
		{
			TextureToolWindow window = (TextureToolWindow)EditorWindow.GetWindow(typeof(TextureToolWindow));
			window.minSize = new Vector2(414,333);
			window.Show();

			return window;
		}

		private void OnGUI()
		{
			EditorGUILayout.BeginVertical("box");


			EditorGUIUtility.wideMode = true;

			offset = GetIntVector2(EditorGUILayout.Vector2Field("Offset",offset),0,0);
			size = GetIntVector2(EditorGUILayout.Vector2Field("Size",size),1,1);

			fade = EditorGUILayout.Toggle("Fade",fade);

			if(fade)
				padding = GetIntVector2(EditorGUILayout.Vector2Field("Padding",padding),2,2);
			else
				padding = GetIntVector2(EditorGUILayout.Vector2Field("Padding",padding),0,0);

			Texture preTex = texture;
			texture = (Texture2D)EditorGUILayout.ObjectField("Texture",texture,typeof(Texture2D),false,GUILayout.Height(200));

			if(preTex != texture || (string.IsNullOrEmpty(saveFolder) && texture != null))
			{
				saveFolder = GetFolderPath(texture);
			}

			if(preTex != texture && texture != null)
			{
				saveName = texture.name + "_sliced";
			}


			EditorGUILayout.BeginHorizontal();
			saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
			if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.Width(22)))
			{
				saveFolder = EditorUtility.SaveFolderPanel("Folder to Save asset", saveFolder, "");
				GUIUtility.keyboardControl = 0;
			}
			EditorGUILayout.EndHorizontal();

			saveName = EditorGUILayout.TextField("Save Name",saveName);

			if(GUILayout.Button("Slice"))
			{
				string path = saveFolder + "/" +saveName + ".png";
				SliceTexture(texture,offset,size,padding,fade,path);
			}

			EditorGUILayout.EndVertical();
		}

		private Vector2 GetIntVector2(Vector2 v,int minX,int minY)
		{
			return new Vector2(Mathf.Clamp((int)v.x,minX,int.MaxValue),Mathf.Clamp((int)v.y,minY,int.MaxValue));
		}

		public static string GetFolderPath(UnityEngine.Object obj)
		{
			return System.IO.Path.GetDirectoryName(RailGunEditorUtility.GetDiskFullPathFromAssetPath(AssetDatabase.GetAssetPath(obj)));
		}
		public static Texture2D SliceTexture(Texture2D texture,Vector2 offset,Vector2 size,Vector2 padding,bool fade,string savePath)
		{
			if(texture != null)
			{
				SetTextureImporterFormat(texture);

				int width = texture.width;
				int height = texture.height;
				Texture2D newTexture = new Texture2D(width,height,TextureFormat.RGBA32,false);

				Color[] colors = texture.GetPixels();
				Color[] newColors = texture.GetPixels();

				for(int i = 0;i < newColors.Length;i++)
				{
					newColors[i].r = 1;
					newColors[i].g = 1;
					newColors[i].b = 1;
					newColors[i].a = 0;
				}
				newTexture.SetPixels(newColors);

				int offsetX = (int)offset.x;
				int offsetY = (int)offset.y;
				int sizeX = (int)size.x;
				int sizeY = (int)size.y;
				//texture 是从左下角开始一行一行叠上来计算的
				//我们的offset及padding是从左上角开始算的
				for(int row = height - 1;row >= 0;row--)
				{
					for(int col = 0;col < width;col++)
					{
						int currentIndex = RowCol2Index(row,col,width,height);

						int invertRow = height - row - 1;

						int targetRow = row - offsetY - invertRow / sizeY * (int)padding.y;
						int targetCol = col + offsetX + col / sizeX * (int)padding.x;
						int targetIndex = RowCol2Index(targetRow,targetCol,width,height);

						if(targetIndex >= 0)
						{
							Color currentColor = colors[currentIndex];
							newColors[targetIndex] = currentColor;

							if(fade)
							{
								bool top = ((invertRow % sizeY) == 0);
								bool bottom = (((invertRow + 1) % sizeY) == 0);
								bool left = ((col % sizeY) == 0);
								bool right = (((col + 1) % sizeY) == 0);
								int kPadding = Mathf.FloorToInt( padding.y/2);

								if(top)
								{
									DrawFadeGrid(newColors,targetRow+1,kPadding,targetCol,0,width,height,currentColor);
									if(left)
										DrawFadeGrid(newColors,targetRow+1,kPadding,targetCol - kPadding,kPadding,width,height,currentColor);
									if(right)
										DrawFadeGrid(newColors,targetRow+1,kPadding,targetCol+1,kPadding,width,height,currentColor);
								}
								if(bottom)
								{
									DrawFadeGrid(newColors,targetRow-kPadding-1,kPadding,targetCol,0,width,height,currentColor);
									if(left)
										DrawFadeGrid(newColors,targetRow-kPadding-1,kPadding,targetCol - kPadding,kPadding,width,height,currentColor);
									if(right)
										DrawFadeGrid(newColors,targetRow-kPadding-1,kPadding,targetCol+1,kPadding,width,height,currentColor);
								}
								if(left)
								{
									DrawFadeGrid(newColors,targetRow,0,targetCol - kPadding,kPadding,width,height,currentColor);
								}
								if(right)
								{
									DrawFadeGrid(newColors,targetRow,0,targetCol+1,kPadding,width,height,currentColor);
								}
							}
						}


					}
				}
				newTexture.SetPixels(newColors);
				byte[] bytes = newTexture.EncodeToPNG();

				System.IO.File.WriteAllBytes(savePath, bytes);

				AssetDatabase.Refresh();
				Debug.Log(savePath);

				Texture2D loadTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(RailGunEditorUtility.GetAssetPathFromDiskPath(savePath));
				return loadTexture;
			}

			return null;
		}

		private static void DrawFadeGrid(Color[] newColors,int row,int rowPadding,int col,int colPadding,int width,int height,Color color)
		{
			for (int i=row; i<=row + rowPadding; i++) {
				for (int j = col; j <= col + colPadding; j++) {
					int targetIndex = RowCol2Index(i,j,width,height);
					if(targetIndex >= 0)
					{
						newColors[targetIndex] = color;
//						newColors[targetIndex] = Color.red;
					}
				}
			}
		}

		private static int RowCol2Index(int row,int col,int width,int height)
		{
			if(row < 0 || row >= height || col < 0 || col >= width)
				return -1;
			else
				return row * width + col;
		}

		public static void SetTextureImporterFormat( Texture2D texture)
		{
			if ( null == texture ) return;

			string assetPath = AssetDatabase.GetAssetPath( texture );
			var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
			if ( tImporter != null && !tImporter.isReadable)
			{
				tImporter.isReadable = true;

				AssetDatabase.ImportAsset( assetPath );
				AssetDatabase.Refresh();
			}
		}
	}
}

