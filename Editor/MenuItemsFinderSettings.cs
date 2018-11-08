using UnityEngine;

namespace SKTools.MenuItemsFinder
{
	[CreateAssetMenuAttribute(fileName = "Settings", order = 2000, menuName = "SKTools/Create MenuItemsFinder Settings")]
	internal class MenuItemsFinderSettings : ScriptableObject 
	{
     	[SerializeField] private Color _itemStarredColor = Color.green;
		[SerializeField] private Color _itemDefaultColor = Color.white;
		[SerializeField] private Color _itemMissedColor = Color.red;
		[SerializeField] private Color _itemNotExecutableColor = Color.gray;
		[SerializeField] private Color _itemSelectedContentColor = Color.yellow;
		[SerializeField] private Color _itemHotKeyColor = Color.cyan;

		public Color ItemStarredColor
		{
			get { return _itemStarredColor; }
		}

		public Color ItemDefaultColor
		{
			get { return _itemDefaultColor; }
		}

		public Color ItemMissedColor
		{
			get { return _itemMissedColor; }
		}

		public Color ItemNotExecutableColor
		{
			get { return _itemNotExecutableColor; }
		}

		public Color ItemSelectedContentColor
		{
			get { return _itemSelectedContentColor; }
		}

		public Color ItemHotKeyColor
		{
			get { return _itemHotKeyColor; }
		}
	}
}

