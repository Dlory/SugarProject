using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RailGun
{
	/// <summary>
	/// 将多个2d polygon结合成一个，直到无法结合为止
	/// </summary>
	public class Polygon2DCombiner
	{
		public static Vector2[][] CombinePolygons(Vector2[][] polygons)
		{
			if(polygons.Length <= 1)
				return polygons;
			else
			{
				bool allAreIndependent;
				List<Vector2[]> list = new List<Vector2[]>(polygons);
				do
				{
					allAreIndependent = true;
					List<Vector2[]> newList = new List<Vector2[]>();

					for(int i = 0;i < list.Count;)
					{
						Vector2[][] ret;

						if(i + 1 >= list.Count)
						{
							//第i个是最后一个了，不用合并操作
							newList.Add(list[i]);
							i++;
						}
						else
						{
							if(CombineTwoPolygons(list[i],list[i + 1],out ret))
							{
								allAreIndependent = false;
								newList.Add(ret[0]);
								i += 2;
							}
							else
							{
								newList.Add(ret[0]);
								i++;
							}
						}
					}

					list = newList;
				}
				while(!allAreIndependent && list.Count > 1);

				return list.ToArray();
			}
		}

		/// <summary>
		/// 如果ab相交或者包含，则返回true，合并成一个
		/// 如果两个独立，则返回false，返回两个数组,这两个数组是经过规范化处理的
		/// </summary>
		private static bool CombineTwoPolygons(Vector2[] pa,Vector2[] pb,out Vector2[][] ret)
		{
			List<PolygonPoint> pointsA = GeneratePolygonPoint(pa);//NormalizePolygonPoints(pa);
			List<PolygonLine> linesA = GeneratePolygonLine(pointsA);

			List<PolygonPoint> pointsB = GeneratePolygonPoint(pb);//NormalizePolygonPoints(pb);
			List<PolygonLine> linesB = GeneratePolygonLine(pointsB);

			bool cross = TestAllSegmentsCrossing(linesA,linesB);

			if(cross)
			{
				//有相交的线段分割成小的
				List<PolygonLine> finalLinesA = SpiltCrossLines(linesA);
				List<PolygonLine> finalLinesB = SpiltCrossLines(linesB);

				//每个点设置好出去的线段
				SetPointsTheirLines(finalLinesA);
				SetPointsTheirLines(finalLinesB);

				pointsA = ChangeLinesToPoints(finalLinesA,false);
				pointsB = ChangeLinesToPoints(finalLinesB,false);

				List<PolygonPoint> allPoint = new List<PolygonPoint>();
				allPoint.AddRange(pointsA);
				allPoint.AddRange(pointsB);

				PolygonPoint leftBottom = FindLeftBottomPoint(allPoint);

				List<PolygonPoint> finalPolygonPoints = FindCombinePoints(leftBottom);

				Vector2[] finalPoints = new Vector2[finalPolygonPoints.Count];
				for(int i = 0;i < finalPolygonPoints.Count;i++)
				{
					finalPoints[i] = finalPolygonPoints[i].position;
				}
				ret = new Vector2[1][];
				ret[0] = finalPoints;

				return true;
			}
			else
			{
				bool aContainB = PolygonContainsPoint(linesA,pointsB[0]);

				if(aContainB)
				{
					ret = new Vector2[1][];
					ret[0] = pa;
					return true;
				}
				else
				{
					bool bContainA = PolygonContainsPoint(linesB,pointsA[0]);
					if(bContainA)
					{
						ret = new Vector2[1][];
						ret[0] = pb;
						return true;
					}
					else
					{
						ret = new Vector2[2][];
						ret[0] = pa;
						ret[1] = pb;
						return false;
					}
				}


			}
		}

		private static bool PolygonContainsPoint(List<PolygonLine> polygon,PolygonPoint point)
		{
			float minX = polygon[0].pointA.position.x;
			float maxX = polygon[0].pointA.position.x;
			float minY = polygon[0].pointA.position.y;
			float maxY = polygon[0].pointA.position.y;
			for ( int i = 1 ; i < polygon.Count ; i++ )
			{
				Vector2 q = polygon[i].pointA.position;
				minX = Mathf.Min( q.x, minX );
				maxX = Mathf.Max( q.x, maxX );
				minY = Mathf.Min( q.y, minY );
				maxY = Mathf.Max( q.y, maxY );
			}

			if (point.position.x < minX || point.position.x > maxX || point.position.y < minY || point.position.y > maxY)
			{
				return false;
			}

			// http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
			bool inside = false;
			Vector2 p = point.position;

			for ( int i = 0, j = polygon.Count - 1 ; i < polygon.Count ; j = i++ )
			{
				Vector2 polygonI = polygon[i].pointA.position;
				Vector2 polygonJ = polygon[j].pointA.position;
				if ((polygonI.y > p.y ) != ( polygonJ.y > p.y ) &&
					p.x < ( polygonJ.x - polygonI.x ) * ( p.y - polygonI.y ) / ( polygonJ.y - polygonI.y ) + polygonI.x)
				{
					inside = !inside;
				}
			}

			return inside;
		}

		private static List<PolygonLine> SpiltCrossLines(List<PolygonLine> lines)
		{
			List<PolygonLine> newLines = new List<PolygonLine>(lines.Count);
			for(int i = 0;i < lines.Count;i++)
			{
				PolygonLine current = lines[i];
				if(current.internalPoints.Count == 0)
					newLines.Add(current);
				else
				{
					current.SortInternalPoints();

					PolygonPoint startPoint = current.pointA;
					for(int k = 0;k < current.internalPoints.Count;k++)
					{
						PolygonLine newLine = new PolygonLine(startPoint,current.internalPoints[k]);
						newLines.Add(newLine);

						startPoint = current.internalPoints[k];
					}

					PolygonLine endLine = new PolygonLine(startPoint,current.pointB);
					newLines.Add(endLine);
				}
			}
			return newLines;
		}

		/// <summary>
		/// 每个顶点设置好以他为出发点的线段
		/// </summary>
		private static void SetPointsTheirLines(List<PolygonLine> lines)
		{
			for(int i = 0;i < lines.Count;i++)
			{
				PolygonPoint point = lines[i].pointA;
				point.ownLines.Add(lines[i]);
			}
		}

		private static PolygonPoint FindLeftBottomPoint(List<PolygonPoint> list)
		{
			PolygonPoint leftBottom = list[0];
			Vector2 min = leftBottom.position;

			//先找到(minX,minY)
			for(int i = 1;i < list.Count;i++)
			{
				PolygonPoint current = list[i];
				if(current.position.x < min.x)
					min.x = current.position.x;

				if(current.position.y < min.y)
					min.y = current.position.y;
			}

			float minDistance = Vector2.Distance(leftBottom.position,min);
			for(int i = 1;i < list.Count;i++)
			{
				PolygonPoint current = list[i];
				float distance = Vector2.Distance(current.position,min);
				if(distance < minDistance)
				{
					leftBottom = current;
					minDistance = distance;
				}
			}

			return leftBottom;
		}
		/// <summary>
		/// 从左下角顶点开始遍历线段，取合并后的点，知道左下角点，返回顶点数组
		/// </summary>
		private static List<PolygonPoint> FindCombinePoints(PolygonPoint leftBottomPoint)
		{
			List<PolygonPoint> list = new List<PolygonPoint>();

			PolygonPoint startPoint = leftBottomPoint;
			list.Add(leftBottomPoint);

			Vector2 preLineDirection = Vector2.right;

			do
			{
				if(startPoint.ownLines.Count == 1)
				{
					startPoint.ownLines[0].visited = true;
					startPoint = startPoint.ownLines[0].pointB;
					list.Add(startPoint);

				}
				else
				{
					PolygonLine bestLine = null;
					for(int i = 0 ; i < startPoint.ownLines.Count;i++)
					{
						PolygonLine currentLine = startPoint.ownLines[i];
						if(!currentLine.visited)
						{
							if(bestLine == null)
								bestLine = currentLine;
							else
							{
								//计算当前line和bestline与preLine角度，选择逆时针角度最大的那条
								Vector2 bestLineDirection = (bestLine.pointB.position - bestLine.pointA.position).normalized;
								Vector2 currentLineDirection = (currentLine.pointB.position - currentLine.pointA.position).normalized;

								float bestLineAngle = Vector2.Angle(bestLineDirection,preLineDirection);
								float currentLineAngle = Vector2.Angle(currentLineDirection,preLineDirection);

								//叉积有方向 aXb = -(bXa)
								Vector3 bestLineCross = Vector3.Cross((Vector3)preLineDirection,(Vector3)bestLineDirection);
								Vector3 currentLineCross = Vector3.Cross((Vector3)preLineDirection,(Vector3)currentLineDirection);
								if(bestLineCross.z < 0)
									bestLineAngle = 360 - bestLineAngle;

								if(currentLineCross.z < 0)
									currentLineAngle = 360 - currentLineAngle;

								if(currentLineAngle > bestLineAngle)
								{
									bestLine = currentLine;
								}
							}
						}
					}

					bestLine.visited = true;
					startPoint = bestLine.pointB;
					preLineDirection = (bestLine.pointB.position - bestLine.pointA.position).normalized;

					if(startPoint != leftBottomPoint)
						list.Add(startPoint);
				}
			}
			while(startPoint != leftBottomPoint && startPoint.HasUnVisitedLine());

			return list;
		}

//		/// <summary>
//		/// 将顶点数组规范化。即有些顶点数组自身就有可能有相交的地方，相交的点没有在顶点数组里，把这些点找出来加进去
//		/// </summary>
//		private static List<PolygonPoint> NormalizePolygonPoints(Vector2[] array)
//		{
//			List<PolygonPoint> points = GeneratePolygonPoint(array);
//			List<PolygonLine> lines = GeneratePolygonLine(points);
//
//			///每一条边都和除自己以外的其他边做相交测试
//			for(int i = 0;i < lines.Count;i++)
//			{
//				for(int k = i + 1;k < lines.Count;k++)
//				{
//					Vector2 intersectPoint;
//					PolygonLine lineA = lines[i];
//					PolygonLine lineB = lines[k];
//					if(Math3D.AreLineSegmentsCrossing(out intersectPoint,lineA.pointA.position,lineA.pointB.position,lineB.pointA.position,lineB.pointB.position))
//					{
//						PolygonPoint point = new PolygonPoint(intersectPoint);
//						lineA.AddInternalPoints(point);
//						lineB.AddInternalPoints(point);
//					}
//				}
//			}
//
//			return ChangeLinesToPoints(lines,true);
//		}

		/// <summary>
		/// 测试两个polygon的线段相交
		/// 如果根本没有相交的线段，则返回false
		/// </summary>
		private static bool TestAllSegmentsCrossing(List<PolygonLine> linesA,List<PolygonLine> linesB)
		{
			bool cross = false;

			for(int i = 0;i < linesA.Count;i++)
			{
				for(int k = 0;k < linesB.Count;k++)
				{
					PolygonLine lineA = linesA[i];
					PolygonLine lineB = linesB[k];

					if(TestSegmentCrossing(lineA,lineB,true) && !cross)
					{
						cross = true;
					}
				}
			}

			return cross;
		}

		private static bool TestSegmentCrossing(PolygonLine lineA,PolygonLine lineB,bool addIntersectPoint)
		{
			Vector2 intersectPoint;
			bool cross = Math3D.AreLineSegmentsCrossing(out intersectPoint,lineA.pointA.position,lineA.pointB.position,lineB.pointA.position,lineB.pointB.position);
			if(cross && addIntersectPoint)
			{
				PolygonPoint point = new PolygonPoint(intersectPoint);
				lineA.AddInternalPoints(point);
				lineB.AddInternalPoints(point);
			}

			return cross;
		}

		private static List<PolygonPoint> GeneratePolygonPoint(Vector2[] array)
		{
			List<PolygonPoint> list = new List<PolygonPoint>(array.Length);

			for(int i = 0;i < array.Length;i++)
			{
				PolygonPoint point = new PolygonPoint(array[i]);

				list.Add(point);
			}

			return list;
		}

		private static List<PolygonLine> GeneratePolygonLine(List<PolygonPoint> array)
		{
			List<PolygonLine> lines = new List<PolygonLine>(array.Count);
			for(int i = 0;i < array.Count;i++)
			{
				PolygonPoint pointA = array[i];
				PolygonPoint pointB;
				if(i + 1 >= array.Count)
					pointB = array[0];
				else
					pointB = array[i + 1];

				lines.Add(new PolygonLine(pointA,pointB));
			}

			return lines;
		}

		/// <summary>
		/// 将线段数组转化为顶点数组，首尾点一定要是同一个，形成闭环
		/// </summary>
		private static List<PolygonPoint> ChangeLinesToPoints(List<PolygonLine> lines,bool addInternalPoints)
		{
			if(lines.Count < 3 || lines[0].pointA != lines[lines.Count - 1].pointB)
			{
				return null;
			}
			else
			{
				List<PolygonPoint> list = new List<PolygonPoint>();
				for(int i = 0;i < lines.Count;i++)
				{
					PolygonLine line = lines[i];

					list.Add(line.pointA);
					if(addInternalPoints)
					{
						line.SortInternalPoints();
						for(int k = 0;k < line.internalPoints.Count;k++)
						{
							list.Add(new PolygonPoint(line.internalPoints[k].position));
						}
					}
				}

				return list;
			}
		}

		/// <summary>
		/// 平滑点，如果两个点之间距离小于判断距离并且夹角小于设定的值，则合并为一个点
		/// 输入最少三个点，输出最少也是三个点
		/// </summary>
		public static Vector2[] FlattenPoints(Vector2[] points,float degree = 180.0f,float judgeDistance = 0.1f)
		{
			if(points == null || points.Length <= 3)
				return points;
			else
			{
				List<Vector2> list = new List<Vector2>(points.Length);
				list.Add(points[0]);

				Vector2 beforePoint = points[0];
				int lastAddIndex = 1;
				for(int i = 1;i < points.Length;i++)
				{
					Vector2 currentPoint = points[i];
					Vector2 nextPoint;
					if(i + 1 >= points.Length)
						nextPoint = points[0];
					else
						nextPoint = points[i + 1];

					float angle = Vector2.Angle(currentPoint - beforePoint,nextPoint - currentPoint);
					float distance = Vector2.Distance(currentPoint,beforePoint);

					if(angle <= degree && distance <= judgeDistance)
					{
						
					}
					else
					{
						list.Add(currentPoint);
						beforePoint = currentPoint;
						lastAddIndex = i;
					}
				}

				if(list.Count == 1)
				{
					list.Add(points[1]);
					list.Add(points[2]);
				}
				else if(list.Count == 2)
				{
					if(lastAddIndex + 1 < points.Length)
						list.Add(points[lastAddIndex + 1]);
					else
						list.Add(points[lastAddIndex - 1]);
				}

				return list.ToArray();
			}
		}
	}

	class PolygonPoint
	{
		public Vector2 position;

		public List<PolygonLine> ownLines = new List<PolygonLine>();

		public PolygonPoint(Vector2 point)
		{
			position = point;
		}

		/// <summary>
		/// 有没有还没有访问过的线段
		/// </summary>
		public bool HasUnVisitedLine()
		{
			bool allVisited = true;
			for(int i = 0;i < ownLines.Count;i++)
			{
				if(!ownLines[i].visited)
				{
					allVisited = false;
					break;
				}
			}

			return !allVisited;
		}

		public override string ToString()
		{
			return string.Format("{0},{1}",position.x,position.y);
		}
	}

	class PolygonLine
	{
		public bool visited;
		public PolygonPoint pointA;
		public PolygonPoint pointB;

		public List<PolygonPoint> internalPoints = new List<PolygonPoint>();

		public PolygonLine(PolygonPoint a,PolygonPoint b)
		{
			pointA = a;
			pointB = b;
		}

		/// <summary>
		/// 将点按照距离从近到远排序
		/// </summary>
		public void SortInternalPoints()
		{
			internalPoints.Sort(SortCompare);
		}

		private int SortCompare(PolygonPoint a,PolygonPoint b)
		{
			float distanceA = Vector2.Distance(pointA.position,a.position);
			float distanceB = Vector2.Distance(pointA.position,b.position);

			//返回正值表示排在前
			float compare = distanceB - distanceA;
			if(compare == 0)
				return 0;
			else if(compare > 0)
				return -1;
			else
				return 1;
		}


		public void AddInternalPoints(PolygonPoint point)
		{
			internalPoints.Add(point);
		}
	}
}

