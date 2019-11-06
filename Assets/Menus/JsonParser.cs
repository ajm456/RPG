using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class JsonParser
{
	private const string JSON_ROOT = "Assets/Menus/";

	private enum WidgetType
	{
		BUTTON,
		TEXT,

		INVALID
	}

	sealed class JsonWidget
	{
		public int id;
		public string type;
		public float x, y, w, h;

		public JsonWidget(int id, string type, float x, float y, float w, float h) {
			this.id = id;
			this.type = type;
			this.x = x;
			this.y = y;
			this.w = w;
			this.h = h;
		}
	}

	public static void LoadWidgets() {
		string json = File.ReadAllText(JSON_ROOT + "widgets.json");
		JsonWidget widgets = JsonUtility.FromJson<JsonWidget>(json);
		Debug.Log(widgets.ToString());
		Debug.Break();
	}
}
