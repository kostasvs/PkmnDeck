using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilterSort : MonoBehaviour {

	public static FilterSort Me { get; private set; }

	public Image filterIcon;
	private Color filterInitCol;
	public Color filterSelectedCol;

	public ButtonList superList;
	public ButtonList subList;
	public ButtonList typeList;
	public ButtonList rarityList;

	void Awake () {

		Me = this;
		filterInitCol = filterIcon.color;
	}

	public static void CreateMenus () {

		if (Me) Me.MyCreateMenus ();
	}

	private void MyCreateMenus () {

		var foundSupertypes = new HashSet<string> ();
		var foundSubtypes = new HashSet<string> ();
		var foundTypes = new HashSet<string> ();
		var foundRarities = new HashSet<string> ();

		foreach (var c in Cards.Me.cards) {

			// supertype
			if (!string.IsNullOrEmpty (c.supertype)) {
				foundSupertypes.Add (c.supertype);
			}

			// subtype
			if (c.subtypes != null) {
				foreach (var st in c.subtypes) {
					if (!string.IsNullOrEmpty (st)) {
						foundSubtypes.Add (st);
					}
				}
			}

			// type
			if (c.types != null) {
				foreach (var st in c.types) {
					if (!string.IsNullOrEmpty (st)) {
						foundTypes.Add (st);
					}
				}
			}

			// rarity
			if (!string.IsNullOrEmpty (c.rarity)) {
				foundRarities.Add (c.rarity);
			}
		}

		// make lists

		// supertype
		superList.ClearList ();
		foreach (var i in foundSupertypes) {

			var ii = i;
			var b = superList.AddButton (ii);
			b.SetAction (() => {

				if (b.IsSelected ()) {
					b.SetSelected (false);
					CardList.Me.filterSupertype = string.Empty;
				}
				else {
					b.SetSelectedExclusive (true);
					CardList.Me.filterSupertype = ii;
				}
				CardList.Me.UpdateFilters ();
			});
		}

		// supertype
		superList.ClearList ();
		foreach (var i in foundSupertypes) {

			var ii = i;
			var b = superList.AddButton (ii);
			b.SetAction (() => {

				if (b.IsSelected ()) {
					b.SetSelected (false);
					CardList.Me.filterSupertype = string.Empty;
				}
				else {
					b.SetSelectedExclusive (true);
					CardList.Me.filterSupertype = ii;
				}
				UpdateFilters ();
			});
		}

		// subtype
		subList.ClearList ();
		foreach (var i in foundSubtypes) {

			var ii = i;
			var b = subList.AddButton (ii);
			b.SetAction (() => {

				if (b.IsSelected ()) {
					b.SetSelected (false);
					CardList.Me.filterSubtype = string.Empty;
				}
				else {
					b.SetSelectedExclusive (true);
					CardList.Me.filterSubtype = ii;
				}
				UpdateFilters ();
			});
		}

		// type
		typeList.ClearList ();
		foreach (var i in foundTypes) {

			var ii = i;
			var b = typeList.AddButton (ii);
			b.SetAction (() => {

				if (b.IsSelected ()) {
					b.SetSelected (false);
					CardList.Me.filterType = string.Empty;
				}
				else {
					b.SetSelectedExclusive (true);
					CardList.Me.filterType = ii;
				}
				UpdateFilters ();
			});
		}

		// rarity
		foreach (var i in foundRarities) {

			var ii = i;
			var b = rarityList.AddButton (ii);
			b.SetAction (() => {

				if (b.IsSelected ()) {
					b.SetSelected (false);
					CardList.Me.filterRarity = string.Empty;
				}
				else {
					b.SetSelectedExclusive (true);
					CardList.Me.filterRarity = ii;
				}
				UpdateFilters ();
			});
		}
	}

	public static void UpdateFilters () {

		if (CardList.Me) CardList.Me.UpdateFilters ();

		if (!Me) return;

		bool hasFilters = !string.IsNullOrEmpty (CardList.Me.filterSupertype) ||
			!string.IsNullOrEmpty (CardList.Me.filterSubtype) ||
			!string.IsNullOrEmpty (CardList.Me.filterType) ||
			!string.IsNullOrEmpty (CardList.Me.filterRarity);

		Me.filterIcon.color = hasFilters ? Me.filterSelectedCol : Me.filterInitCol;
	}
}
