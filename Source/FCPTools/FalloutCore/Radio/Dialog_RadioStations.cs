using UnityEngine;

namespace FCP.Core.Radio;

public class Dialog_RadioStations : Window
{
    private readonly List<RadioStationDef> stations;

    public override Vector2 InitialSize => new Vector2(380f, 460f);

    public Dialog_RadioStations(List<RadioStationDef> stations)
    {
        this.stations = stations;
        doCloseX = true;
        forcePause = false;
        absorbInputAroundWindow = true;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Listing_Standard listing = new Listing_Standard();
        listing.Begin(inRect);

        Text.Font = GameFont.Medium;
        listing.Label("FCP_Radio_TuneStation".Translate());
        Text.Font = GameFont.Small;
        listing.GapLine();

        SongDef current = Find.MusicManagerPlay.CurrentSong;
        if (current != null)
            listing.Label("FCP_Radio_NowPlaying".Translate(current.LabelCap));

        listing.Gap();

        foreach (RadioStationDef station in stations)
        {
            Text.Font = GameFont.Medium;
            listing.Label(station.LabelCap);
            Text.Font = GameFont.Small;

            foreach (SongDef song in station.songs)
            {
                if (listing.ButtonText(song.LabelCap))
                {
                    Find.MusicManagerPlay.ForcePlaySong(song, false);
                    Close();
                }
            }
            listing.Gap();
        }

        listing.End();
    }
}
