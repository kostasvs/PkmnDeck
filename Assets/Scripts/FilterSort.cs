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

	public ButtonList sortList;
	public static int sorting = -1;

	public static readonly string[] raritySort = new string[] {
		"Rare Holo",
		"Rare",
		"Uncommon",
		"Common",
		"",
	};

	void Awake () {

		Me = this;
		filterInitCol = filterIcon.color;

		// sort dex number
		var bm1 = sortList.AddButton ("Pokedex number");
		bm1.SetAction (() => {

			if (bm1.IsSelected ()) return;
			bm1.SetSelectedExclusive (true);
			SetSortType (-1);
		});
		// set as default sort
		bm1.SetSelected (true);

		// sort name
		var b0 = sortList.AddButton ("Name");
		b0.SetAction (() => {

			if (b0.IsSelected ()) return;
			b0.SetSelectedExclusive (true);
			SetSortType (0);
		});

		// sort supertype
		var b1 = sortList.AddButton ("Supertype");
		b1.SetAction (() => {

			if (b1.IsSelected ()) return;
			b1.SetSelectedExclusive (true);
			SetSortType (1);
		});

		// sort subtype
		var b2 = sortList.AddButton ("Subtype");
		b2.SetAction (() => {

			if (b2.IsSelected ()) return;
			b2.SetSelectedExclusive (true);
			SetSortType (2);
		});

		// sort type
		var b3 = sortList.AddButton ("Type");
		b3.SetAction (() => {

			if (b3.IsSelected ()) return;
			b3.SetSelectedExclusive (true);
			SetSortType (3);
		});

		// sort hp
		var b4 = sortList.AddButton ("HP");
		b4.SetAction (() => {

			if (b4.IsSelected ()) return;
			b4.SetSelectedExclusive (true);
			SetSortType (4);
		});

		// sort rarity
		var b5 = sortList.AddButton ("Rarity");
		b5.SetAction (() => {

			if (b5.IsSelected ()) return;
			b5.SetSelectedExclusive (true);
			SetSortType (5);
		});
	}

	public void SetSortType (int sort) {

		if (sorting == sort) return;
		sorting = sort;
		if (CardList.Me) CardList.Me.SortCards (sorting);
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
