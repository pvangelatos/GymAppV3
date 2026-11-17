// Shared FullCalendar options for every calendar in the app.
// Pages spread these first and then add their own toolbar, events and handlers.
window.gymCalendarDefaults = {
    initialView: 'timeGridWeek',
    locale: 'el',
    firstDay: 1,                    // week starts on Monday
    height: 'auto',                 // grow with content, no inner scroll
    slotMinTime: '07:00:00',        // gym opening hours
    slotMaxTime: '22:00:00',
    allDaySlot: false,
    nowIndicator: true,
    eventDisplay: 'block',
    slotLabelFormat: { hour: '2-digit', minute: '2-digit', hour12: false },
    eventTimeFormat: { hour: '2-digit', minute: '2-digit', hour12: false },
    titleFormat: { day: 'numeric', month: 'long', year: 'numeric' },
    dayHeaderFormat: { weekday: 'short', day: 'numeric', month: 'numeric' },
    buttonText: { today: 'Σήμερα', week: 'Εβδομάδα', month: 'Μήνας' }
};