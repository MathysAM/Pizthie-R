using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using PizthieR.Controller;

namespace PizthieR;

public partial class Programmation : UserControl
{
    public Programmation()
    {
        InitializeComponent();
    }

    private sealed class Day
    {
        public CheckBox Active { get; init; } = default!;
        public ComboBox StartHour { get; init; } = default!;
        public ComboBox StartMinute { get; init; } = default!;
        public ComboBox StopHour { get; init; } = default!;
        public ComboBox StopMinute { get; init; } = default!;
        public CheckBox ActiveMatin { get; init; } = default!;
        public ComboBox StartHourMatin { get; init; } = default!;
        public ComboBox StartMinuteMatin { get; init; } = default!;
        public ComboBox StopHourMatin { get; init; } = default!;
        public ComboBox StopMinuteMatin { get; init; } = default!;
    }

    private readonly List<Day> _days = new();
    private readonly int[] _hoursSoir  = Enumerable.Range(13, 8).ToArray();  // 13h–20h
    private readonly int[] _hoursMatin = Enumerable.Range(6,  8).ToArray();  // 6h–13h
    private readonly int[] _minutes = Enumerable.Range(0, 60).Where(m => m % 5 == 0).ToArray();
 

    MqttController _mqtt;
    public Programmation(MqttController _mqtt)
    {
        InitializeComponent();
        this._mqtt = _mqtt;

        InitDays();

        // Remplit les ComboBox et branche la validation
        for (int i = 0; i < _days.Count; i++)
        {
            var d = _days[i];

            
            // AVANT (erreur CS0200) :
            // d.StartHour.Items   = _hours;

            // APRÈS :
            d.StartHour.ItemsSource = _hoursSoir;
            d.StopHour.ItemsSource = _hoursSoir;
            d.StartMinute.ItemsSource = _minutes;
            d.StopMinute.ItemsSource = _minutes;
            d.StartHourMatin.ItemsSource = _hoursMatin;
            d.StopHourMatin.ItemsSource = _hoursMatin;
            d.StartMinuteMatin.ItemsSource = _minutes;
            d.StopMinuteMatin.ItemsSource = _minutes;

            int idx = i;
            d.StartHour.SelectionChanged += (_, __) => ValidateTime(idx);
            d.StartMinute.SelectionChanged += (_, __) => ValidateTime(idx);
            d.StopHour.SelectionChanged += (_, __) => ValidateTime(idx);
            d.StopMinute.SelectionChanged += (_, __) => ValidateTime(idx);
            d.StartHourMatin.SelectionChanged += (_, __) => ValidateTimeMatin(idx);
            d.StartMinuteMatin.SelectionChanged += (_, __) => ValidateTimeMatin(idx);
            d.StopHourMatin.SelectionChanged += (_, __) => ValidateTimeMatin(idx);
            d.StopMinuteMatin.SelectionChanged += (_, __) => ValidateTimeMatin(idx);
        }

        // Bouton "Enregistrer" → publish tous les jours + retour utilisateur
        BtnSaveAll.Click += async (_, __) =>
        {
            BtnSaveAll.IsEnabled = false;
            bool ok = await SaveAllAsync();
            BtnSaveAll.IsEnabled = true;
            await ShowToastAsync(
                ok ? "✓  Programmation enregistrée" : "⚠  Échec de l'enregistrement",
                ok);
        };

        
    }

    private void InitDays()
    {
        _days.Clear();
        _days.Add(new Day { Active = Active0, StartHour = StartHour0, StartMinute = StartMinute0, StopHour = StopHour0, StopMinute = StopMinute0, ActiveMatin = ActiveMatin0, StartHourMatin = StartHourMatin0, StartMinuteMatin = StartMinuteMatin0, StopHourMatin = StopHourMatin0, StopMinuteMatin = StopMinuteMatin0 });
        _days.Add(new Day { Active = Active1, StartHour = StartHour1, StartMinute = StartMinute1, StopHour = StopHour1, StopMinute = StopMinute1, ActiveMatin = ActiveMatin1, StartHourMatin = StartHourMatin1, StartMinuteMatin = StartMinuteMatin1, StopHourMatin = StopHourMatin1, StopMinuteMatin = StopMinuteMatin1 });
        _days.Add(new Day { Active = Active2, StartHour = StartHour2, StartMinute = StartMinute2, StopHour = StopHour2, StopMinute = StopMinute2, ActiveMatin = ActiveMatin2, StartHourMatin = StartHourMatin2, StartMinuteMatin = StartMinuteMatin2, StopHourMatin = StopHourMatin2, StopMinuteMatin = StopMinuteMatin2 });
        _days.Add(new Day { Active = Active3, StartHour = StartHour3, StartMinute = StartMinute3, StopHour = StopHour3, StopMinute = StopMinute3, ActiveMatin = ActiveMatin3, StartHourMatin = StartHourMatin3, StartMinuteMatin = StartMinuteMatin3, StopHourMatin = StopHourMatin3, StopMinuteMatin = StopMinuteMatin3 });
        _days.Add(new Day { Active = Active4, StartHour = StartHour4, StartMinute = StartMinute4, StopHour = StopHour4, StopMinute = StopMinute4, ActiveMatin = ActiveMatin4, StartHourMatin = StartHourMatin4, StartMinuteMatin = StartMinuteMatin4, StopHourMatin = StopHourMatin4, StopMinuteMatin = StopMinuteMatin4 });
        _days.Add(new Day { Active = Active5, StartHour = StartHour5, StartMinute = StartMinute5, StopHour = StopHour5, StopMinute = StopMinute5, ActiveMatin = ActiveMatin5, StartHourMatin = StartHourMatin5, StartMinuteMatin = StartMinuteMatin5, StopHourMatin = StopHourMatin5, StopMinuteMatin = StopMinuteMatin5 });
        _days.Add(new Day { Active = Active6, StartHour = StartHour6, StartMinute = StartMinute6, StopHour = StopHour6, StopMinute = StopMinute6, ActiveMatin = ActiveMatin6, StartHourMatin = StartHourMatin6, StartMinuteMatin = StartMinuteMatin6, StopHourMatin = StopHourMatin6, StopMinuteMatin = StopMinuteMatin6 });
    }

    // Abonnements MQTT (mêmes topics que ta page Blazor)
    public async Task SubscribeAllAsync()
    {
        for (int i = 0; i < _days.Count; i++)
        {
            int idx = i;

            await _mqtt.SubscribeAsync($"/Prog/{idx}/Active", async (payload, _) =>
            {
                bool val = ParseStrictBool(payload);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _days[idx].Active.IsThreeState = false;
                    _days[idx].Active.IsChecked = val;
                  
                });
            }, 0);

            await _mqtt.SubscribeAsync($"/Prog/{idx}/StartHour", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StartHour, Clamp(v, 13, 20)));
            }, 0);

            await _mqtt.SubscribeAsync($"/Prog/{idx}/StartMinute", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StartMinute, ClampTo5(v)));
            }, 0);

            await _mqtt.SubscribeAsync($"/Prog/{idx}/StopHour", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StopHour, Clamp(v, 13, 20)));
            }, 0);

            await _mqtt.SubscribeAsync($"/Prog/{idx}/StopMinute", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StopMinute, ClampTo5(v)));
            }, 0);

            await _mqtt.SubscribeAsync($"/ProgMatin/{idx}/Active", async (payload, _) =>
            {
                bool val = ParseStrictBool(payload);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _days[idx].ActiveMatin.IsThreeState = false;
                    _days[idx].ActiveMatin.IsChecked = val;
                });
            }, 0);

            await _mqtt.SubscribeAsync($"/ProgMatin/{idx}/StartHour", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StartHourMatin, Clamp(v, 6, 13)));
            }, 0);

            await _mqtt.SubscribeAsync($"/ProgMatin/{idx}/StartMinute", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StartMinuteMatin, ClampTo5(v)));
            }, 0);

            await _mqtt.SubscribeAsync($"/ProgMatin/{idx}/StopHour", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StopHourMatin, Clamp(v, 6, 13)));
            }, 0);

            await _mqtt.SubscribeAsync($"/ProgMatin/{idx}/StopMinute", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StopMinuteMatin, ClampTo5(v)));
            }, 0);
        }
    }

    private static bool ParseStrictBool(string? s)
    {
        if (s is null) return false;
        s = s.Trim();                 // retire espaces/CRLF
        return string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
        // "false" → false ; "0"/"on"/"Active = false" → false ; etc.
    }

    public async Task UnsubscribeAllAsync()
    {
        for (int i = 0; i < _days.Count; i++)
        {
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/Active");
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/StartHour");
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/StartMinute");
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/StopHour");
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/StopMinute");
            await _mqtt.UnsubscribeAsync($"/ProgMatin/{i}/Active");
            await _mqtt.UnsubscribeAsync($"/ProgMatin/{i}/StartHour");
            await _mqtt.UnsubscribeAsync($"/ProgMatin/{i}/StartMinute");
            await _mqtt.UnsubscribeAsync($"/ProgMatin/{i}/StopHour");
            await _mqtt.UnsubscribeAsync($"/ProgMatin/{i}/StopMinute");
        }
    }

    private async Task<bool> SaveAllAsync()
    {
        try
        {
            for (int i = 0; i < _days.Count; i++)
            {
                var d = _days[i];
                bool isActive = d.Active.IsChecked == true;

                int sh = GetSelectedInt(d.StartHour);
                int sm = GetSelectedInt(d.StartMinute);
                int eh = GetSelectedInt(d.StopHour);
                int em = GetSelectedInt(d.StopMinute);

                await _mqtt.PublishActive($"/Prog/{i}/Active", isActive ? "true" : "false");
                await _mqtt.PublishActive($"/Prog/{i}/StartHour", sh.ToString());
                await _mqtt.PublishActive($"/Prog/{i}/StartMinute", sm.ToString());
                await _mqtt.PublishActive($"/Prog/{i}/StopHour", eh.ToString());
                await _mqtt.PublishActive($"/Prog/{i}/StopMinute", em.ToString());

                bool isActiveMatin = d.ActiveMatin.IsChecked == true;
                int shm = GetSelectedInt(d.StartHourMatin);
                int smm = GetSelectedInt(d.StartMinuteMatin);
                int ehm = GetSelectedInt(d.StopHourMatin);
                int emm = GetSelectedInt(d.StopMinuteMatin);

                await _mqtt.PublishActive($"/ProgMatin/{i}/Active", isActiveMatin ? "true" : "false");
                await _mqtt.PublishActive($"/ProgMatin/{i}/StartHour", shm.ToString());
                await _mqtt.PublishActive($"/ProgMatin/{i}/StartMinute", smm.ToString());
                await _mqtt.PublishActive($"/ProgMatin/{i}/StopHour", ehm.ToString());
                await _mqtt.PublishActive($"/ProgMatin/{i}/StopMinute", emm.ToString());
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Affiche une bandeau éphémère en bas de la page (succès = vert, erreur = rouge)
    private async Task ShowToastAsync(string message, bool success)
    {
        ToastText.Text = message;
        Toast.Background = new Avalonia.Media.SolidColorBrush(
            success ? Avalonia.Media.Color.Parse("#1B7F3A")    // vert
                    : Avalonia.Media.Color.Parse("#C62828"));  // rouge
        Toast.IsVisible = true;
        await Task.Delay(2000);
        Toast.IsVisible = false;
    }

    // === Validation : durée ≥ 60 min dans la plage autorisée ===
    private void ValidateTime(int index)
    {
        var d = _days[index];

        int start = GetSelectedInt(d.StartHour) * 60 + GetSelectedInt(d.StartMinute);
        int stop  = GetSelectedInt(d.StopHour)  * 60 + GetSelectedInt(d.StopMinute);
        const int maxStop = 20 * 60;

        if (stop - start < 60)
        {
            int newStop = Math.Min(start + 60, maxStop);
            SelectValue(d.StopHour,   newStop / 60);
            SelectValue(d.StopMinute, ClampTo5(newStop % 60));
        }
    }

    private void ValidateTimeMatin(int index)
    {
        var d = _days[index];

        int start = GetSelectedInt(d.StartHourMatin) * 60 + GetSelectedInt(d.StartMinuteMatin);
        int stop  = GetSelectedInt(d.StopHourMatin)  * 60 + GetSelectedInt(d.StopMinuteMatin);
        const int maxStop = 13 * 60;

        if (stop - start < 60)
        {
            int newStop = Math.Min(start + 60, maxStop);
            SelectValue(d.StopHourMatin,   newStop / 60);
            SelectValue(d.StopMinuteMatin, ClampTo5(newStop % 60));
        }
    }

    // === Helpers UI/valeurs ===
    private static Task UI(Action a) => Dispatcher.UIThread.InvokeAsync(a).GetTask();

    private static void SelectValue(ComboBox combo, int value)
    {
        if (combo.ItemsSource is IEnumerable<int> data)
        {
            int idx = 0;
            int found = -1;
            foreach (var v in data)
            {
                if (v == value) { found = idx; break; }
                idx++;
            }
            combo.SelectedIndex = found >= 0 ? found : 0;
        }
    }

    private static int GetSelectedInt(ComboBox combo)
    {
        if (combo.SelectedItem is int v) return v;

        if (combo.ItemsSource is IEnumerable<int> data)
        {
            // si rien de sélectionné, on prend le 1er
            var e = data.GetEnumerator();
            return e.MoveNext() ? e.Current : 0;
        }
        return 0;
    }

    private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

    private static int ClampTo5(int v)
    {
        v = Clamp(v, 0, 59);
        return v - (v % 5); // 0,5,10,...,55
    }
}
