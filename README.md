# Bachelor-NLP
Ein Client, welcher die Nutzung spezieller Elemente der NLP-Service-Angeboten von AWS, Microsoft, Google und IBM erlaubt.
Einige Funktionalität zur Handhabung der Antworten ist im Quellcode verfügbar.
Dieser Client wurde im Kontext einer Bachelorarbeit entwickelt, 
und enthält außerdem die automatische Teilextraktion von Transparenzdaten aus Datenschutzerklärungen in das [TIL-Format](https://github.com/Transparency-Information-Language/schema).

Um die Services IBM, Google, und Microsoft zu kontaktieren, muss eine config-Datei mit dem Namen "nlpConfig.config" im selben Ordner wie die ausführbare Datei abgelegt werden, welche die entsprechenden Authentifizierungsdaten in folgendem Format enthält:  

ibm_api_key
ibm_service_url
google_api_path
azureCredentials
azureEndpoint
microsoft_standort

In dieser Reihenfolge, beginnend in der ersten Zeile, und danach in jeweils einer neuen Zeile.
Für selektive Services (z.B. wenn nur Microsoft-Authentifizierungsdaten zur Hand sind) die anderen Zeilen leer lassen, aber trotzdem die Zeilenreihenfolge beachten.

Bisher ist eine eigenständige Kompilierung notwendig.
