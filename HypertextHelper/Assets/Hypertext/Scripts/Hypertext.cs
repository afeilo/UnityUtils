﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;


namespace HypertextHelper
{
	//局部文本着色并实现点击
	public abstract class Hypertext:BaseMeshEffect,IPointerClickHandler
	{
		Canvas _rootCanvas;
		Canvas RootCanvas { get { return _rootCanvas ?? (_rootCanvas = GetComponentInParent<Canvas>()); } }

		const int CharVertsNum = 6;
		static readonly ObjectPool<List<UIVertex>> _verticesPool = new ObjectPool<List<UIVertex>>(null, l => l.Clear());
		private List<ClickableEntry> entries = new List<ClickableEntry>();
		public struct ClickableEntry{
			public string Word;
			public int StartIndex;
			public Color Color;
			public Action<string> OnClick;
			public List<Rect> Rects;
			public ClickableEntry(string word,int startIndex,Color color,Action<string> onClick){
				Word = word;
				StartIndex = startIndex;
				Color = color;
				OnClick = onClick;
				Rects = new List<Rect>();
			}
		}

		//超文本信息注册 供子类调用
		protected void RegisterClickable(string word,int startIndex, Color color, Action<string> onClick){
			if (startIndex < 0  || onClick == null) {
				return;
			}
			entries.Add (new ClickableEntry (word,startIndex, color, onClick));
		}
		//超文本信息注册 供子类调用
		protected void RegisterClickable(ClickableEntry clickableEntry){
			entries.Add (clickableEntry);
		}
		//子类继承注册超文本信息
		protected abstract void RegisterClickable ();

		public override void ModifyMesh (VertexHelper vh)
		{
			entries.Clear ();
			RegisterClickable ();			
			var stream = _verticesPool.Get();
			vh.GetUIVertexStream (stream);
			Modify (ref stream);
			vh.AddUIVertexTriangleStream(stream);
			_verticesPool.Release (stream);
		}
		private void Modify(ref List<UIVertex> vertices){
			for (int i = 0,len = entries.Count; i < len; i++) {
				var entry = entries [i];
				for (int textIndex = entry.StartIndex, endIndex = entry.StartIndex + entry.Word.Length; textIndex < endIndex; textIndex++) {
					var vertexTextIndex = textIndex * CharVertsNum;
					Vector2 min = Vector2.one*float.MaxValue;
					Vector2 max = Vector2.one*float.MinValue;//记录文字左上右下坐标
					for (var vertexEndIndex = vertexTextIndex + CharVertsNum; vertexTextIndex < vertexEndIndex; vertexTextIndex++) {
						if (vertexTextIndex >= vertices.Count) {
							break;
						}
						var vertex = vertices [vertexTextIndex];
						vertex.color = entry.Color;
						vertices [vertexTextIndex] = vertex;
						var pos = vertex.position;
						if (pos.x < min.x) {
							min.x = pos.x;
						}
						if (pos.x > max.x) {
							max.x = pos.x;
						}
						if (pos.y < min.y) {
							min.y = pos.y;
						}
						if (pos.y > max.y) {
							max.y = pos.y;
						}
					}
					entry.Rects.Add (new Rect {min = min, max = max});
				}
				List<Rect> rects = new List<Rect>();
				var splitRects = SplitRectsByRow(entry.Rects);
				foreach(List<Rect> rect in splitRects){
					rects.Add (MergeRect (rect));
				}
				entry.Rects = rects;
				entries [i] = entry;
			}

		}
		//同一行的word合并
		List<List<Rect>> SplitRectsByRow(List<Rect> rects)
		{
			List<List<Rect>> splitRects = new List<List<Rect>>();
			int startSplit = 0;
			for (int i = 1,len = rects.Count; i < len; i++) {
				if (rects [i].xMin < rects [i - 1].xMin) {
					splitRects.Add (rects.GetRange (startSplit, i - startSplit));
					startSplit = i;
				}
			}
			if (startSplit < rects.Count) {
				splitRects.Add (rects.GetRange (startSplit, rects.Count - startSplit));
			}
			return splitRects;
		}

		Rect MergeRect(List<Rect> rects){
			Vector2 min = Vector2.one*float.MaxValue;
			Vector2 max = Vector2.one*float.MinValue;
			for (int i = 0, len = rects.Count; i < len; i++) {
				if (rects [i].xMin < min.x) {
					min.x = rects [i].xMin;				
				}
				if (rects [i].yMin < min.y) {
					min.y = rects [i].yMin;				
				}
				if (rects [i].xMax > max.x) {
					max.x = rects [i].xMax;				
				}
				if (rects [i].yMax > max.y) {
					max.y = rects [i].yMax;				
				}
			}
			return new Rect {min = min, max = max};
		}

		Vector3 ToLocalPosition(Vector3 position, Camera camera)
		{
			if (!RootCanvas)
			{
				return Vector3.zero;
			}

			if (RootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				return transform.InverseTransformPoint(position);
			}

			var localPosition = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, position, camera, out localPosition);
			return localPosition;
		}

		void IPointerClickHandler.OnPointerClick (PointerEventData eventData){
			var localPosition = ToLocalPosition(eventData.position, eventData.pressEventCamera);
			foreach (var entry in entries) {
				if (entry.OnClick == null)
				{
					continue;
				}

				foreach (var rect in entry.Rects)
				{
					if (!rect.Contains(localPosition))
					{
						continue;
					}

					entry.OnClick(entry.Word);
					break;
				}
			}
		}
	}
}