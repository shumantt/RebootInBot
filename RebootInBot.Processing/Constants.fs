module RebootInBot.Processing.Constants

let startingCountTemplate = Printf.StringFormat<string->string>("Перезапускаю %s. Никто не против?")
let noMachineStartCount = "Начинаю перезапуск. Никто не против?"
let processAlreadyRunning = "Процесс перезапуска уже запущен, попробуйте позже"
let cancelProcessMessage = "Стоп стоп стоп. Кто-то еще работает"
let errorCancelMessage = "Отсчет не начат или уже завершен. Отменять нечего"
let excludedMentions = "Успешно исключены из списка"
let includeMentions = "Уведомления включены"