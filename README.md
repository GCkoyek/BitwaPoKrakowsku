# BitwaPoKrakowsku  
Wieloosobowa gra akcji oparta na walce bronią białą (Unity / C# / Netcode / Relay)

**BitwaPoKrakowsku** to multiplayerowa gra FPS/TPS stworzona w Unity. Gracze łączą się przez Unity Relay, a synchronizacją zajmuje się Netcode for GameObjects. Projekt zawiera kompletny system lobby, spawnowania, broni, animacji oraz ruchu postaci.

---

## Funkcjonalności

- Multiplayer oparty na **Unity Relay** i **Netcode for GameObjects**  
- Hostowanie i dołączanie do gry za pomocą kodu  
- Stabilne spawnowanie graczy w losowych punktach  
- System broni białej z animacjami zamachu  
- Synchronizacja ruchu, rotacji, animacji i ataków  
- Oddzielne modele FP/TP dla właściciela i pozostałych graczy  
- Automatyczna aktywacja PlayerObject po spawnie (naprawa błędów NGO)  
- UI lobby z obsługą kodu pokoju  

---

## Technologie

- Unity 2022+  
- C#  
- Netcode for GameObjects  
- Unity Relay Services  
- Unity Transport (UTP)  
- Rigidbody FPS Controller  
- RPC (ServerRpc / ClientRpc)  
- NetworkVariable  

---

## Architektura projektu

### 1. RelayManager  
Centralny moduł multiplayer:

- inicjalizacja Unity Services,  
- logowanie anonimowe,  
- tworzenie i dołączanie do Relay,  
- konfiguracja transportu,  
- start hosta/klienta,  
- wymuszenie aktywacji PlayerObject,  
- obsługa callbacków NGO.

Zapewnia stabilne połączenia i eliminuje problemy z nieaktywnymi obiektami po spawnie.

---

### 2. RelayUI  
UI lobby:

- przycisk hostowania,  
- przycisk dołączania,  
- pole na kod pokoju,  
- wyświetlanie kodu hosta.

---

### 3. SpawnManager  
Serwerowy system spawnowania:

- losuje punkt spawnu,  
- ustawia pozycję i rotację gracza,  
- reaguje na OnClientConnected.

---

### 4. System ruchu – RigidbodyFPS  
Kontroler FPS oparty na Rigidbody:

- ruch WSAD, skok, obrót kamery,  
- synchronizacja pozycji i rotacji przez NetworkVariable,  
- interpolacja ruchu graczy niebędących właścicielami,  
- automatyczne przełączanie modeli FP/TP.

---

### 5. System broni – PlayerEquipmentAdvanced  
Odpowiada za:

- losowanie broni,  
- synchronizację wyboru broni,  
- animacje zamachu,  
- RPC do synchronizacji animacji.

Broń opisana jest przez `WeaponData`:

```
name  
weaponObject  
swingAngleX  
swingAngleZ  
swingSpeed  
```

---

### 6. System ataku – PlayerAttackAdvanced  
- lokalne animacje zamachu,  
- RPC do serwera,  
- raycast trafienia,  
- synchronizacja animacji między klientami.

---

### 7. PlayerController  
- aktywacja kamery właściciela,  
- przełączanie modeli FP/TP,  
- blokowanie kursora,  
- losowanie broni przy spawnie.

---

### 8. PlayerAutoEnable  
Naprawia problem NGO, w którym PlayerObject bywa nieaktywny po spawnie:

- wymusza aktywację obiektu,  
- aktywuje wszystkie dzieci.

---

## Struktura projektu

```
/Scripts
    RelayManager.cs
    RelayUI.cs
    SpawnManager.cs
    PlayerController.cs
    PlayerAttackAdvanced.cs
    PlayerEquipmentAdvanced.cs
    RigidbodyFPS.cs
    PlayerAutoEnable.cs
    NetcodeDebug.cs
    WeaponData.cs

/Prefabs
    Player.prefab
    Weapons/

/UI
    LobbyCanvas
```

---

## Przepływ gry

1. Gracz uruchamia grę i widzi lobby.  
2. Host wybiera „Hostuj grę”.  
3. Relay generuje kod pokoju.  
4. Klienci wpisują kod i dołączają.  
5. Serwer spawnuje graczy w losowych punktach.  
6. Każdy gracz otrzymuje losową broń.  
7. Rozpoczyna się walka bronią białą.  

---

## Jak uruchomić projekt

1. Otwórz projekt w Unity.  
2. Upewnij się, że NetworkManager i RelayManager są w scenie lobby.  
3. Przypisz PlayerPrefab w NetworkManager.  
4. Uruchom grę.  
5. Host wybiera „Hostuj”, klienci wpisują kod.  

---

## Wymagania

- Unity 2022+  
- Konto Unity Services  
- NGO + Relay zainstalowane przez Package Manager  

---

## Dalszy rozwój

- system zdrowia i obrażeń,  
- hitboxy zamiast raycastów,  
- animacje postaci,  
- scoreboard,  
- lobby z listą graczy,  
- tryb drużynowy,  
- dedykowany serwer.  
