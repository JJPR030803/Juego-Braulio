# Guía Completa del Sistema de Onion y Pellets de Pikmin

## Tabla de Contenidos
1. [Descripción General](#descripción-general)
2. [Configuración del Sistema Onion](#configuración-del-sistema-onion)
3. [Configuración del Sistema de Pellets](#configuración-del-sistema-de-pellets)
4. [Integración con el Jugador](#integración-con-el-jugador)
5. [Efectos Visuales y Partículas](#efectos-visuales-y-partículas)
6. [Pruebas y Depuración](#pruebas-y-depuración)
7. [Resolución de Problemas](#resolución-de-problemas)

---

## Descripción General

El sistema de Onion de Pikmin incluye las siguientes características:

### **PikminOnion.cs**
- **Generación desde el suelo**: Los Pikmin emergen desde bajo tierra con animación
- **Sistema de activación**: El Onion comienza enterrado y emerge cuando el jugador lo activa
- **Almacenamiento**: Guarda hasta 50 Pikmin (configurable)
- **Sistema de pellets**: Absorbe pellets para crear nuevos Pikmin
- **Auto-spawn**: Opción de generación automática

### **Pellet.cs**
- **Tres tipos de pellets**: Numerados, Flores, Enemigos
- **Sistema de carga**: Los Pikmin pueden cargar pellets
- **Absorción automática**: Detecta cuando está cerca del Onion
- **Valores configurables**: Cada pellet vale diferentes cantidades de Pikmin

---

## Configuración del Sistema Onion

### Paso 1: Crear el GameObject del Onion

1. **Crear un nuevo GameObject vacío**:
   - En la jerarquía: `Clic derecho → 3D Object → Empty` o `Ctrl+Shift+N`
   - Nómbralo `PikminOnion`

2. **Añadir un modelo visual**:
   - Arrastra tu modelo 3D del Onion como hijo del GameObject `PikminOnion`
   - O añade formas básicas: `Clic derecho en PikminOnion → 3D Object → Sphere`
   - Ajusta la escala según necesites (ej: 2, 2, 2)

3. **Añadir el componente PikminOnion**:
   - Selecciona el GameObject `PikminOnion`
   - En el Inspector: `Add Component → Scripts → Pikmin Onion`

4. **Añadir un Collider**:
   - `Add Component → Box Collider` o `Sphere Collider`
   - **IMPORTANTE**: Marca `Is Trigger` si quieres activación por trigger
   - Si NO marcas `Is Trigger`, la activación será por colisión física

### Paso 2: Configurar las Capas (Layers)

1. **Crear capa para el suelo**:
   - Ve a `Edit → Project Settings → Tags and Layers`
   - En `Layers`, encuentra un slot vacío (ej: User Layer 8)
   - Nómbralo `Ground`

2. **Asignar la capa al suelo**:
   - Selecciona tu GameObject de suelo/terreno
   - En el Inspector, cambia `Layer` a `Ground`

### Paso 3: Configurar el Prefab de Pikmin

1. **Crear un Prefab de Pikmin**:
   - Si aún no tienes uno, crea un GameObject con el script `Pikmin.cs`
   - Añade Rigidbody, Collider y el modelo visual
   - Arrástralo a la carpeta `Assets/Prefabs` para crear el prefab
   - Elimina la instancia de la escena

2. **Asignar el Prefab al Onion**:
   - Selecciona el `PikminOnion`
   - En el Inspector, encuentra la sección `Pikmin Spawning`
   - Arrastra tu prefab de Pikmin al campo `Pikmin Prefab`

### Paso 4: Configuración de Spawning (Generación)

#### **Pikmin Spawning**
```
Max Pikmin In Onion: 50          // Máximo de Pikmin almacenados
Current Pikmin Count: 10         // Pikmin iniciales (para pruebas)
Spawn Cooldown: 2.0              // Segundos entre cada spawn
Max Active Spawns: 5             // Cuántos Pikmin pueden emerger simultáneamente
```

#### **Spawn Position & Ground Settings**
```
Spawn Point: (dejar vacío)       // Se usará el transform del Onion
Spawn Radius: 3.0                // Radio donde aparecen los Pikmin
Dig Depth: 2.0                   // Profundidad bajo tierra de spawn
Emerge Speed: 3.0                // Velocidad de emergencia del suelo
Emerge Height: 1.0               // Altura del "pop" al emerger
Ground Layer: Ground             // Selecciona la capa "Ground"
```

**Cómo configurar Ground Layer**:
1. Haz clic en el menú desplegable `Ground Layer`
2. Selecciona `Ground` de la lista
3. Si no aparece, asegúrate de haber creado la capa en el Paso 2

### Paso 5: Configuración de Activación

#### **Activation Settings**
```
Start Deactivated: ✓             // Marcar para empezar enterrado
Buried Depth: 3.0                // Profundidad del Onion enterrado
Rise Speed: 2.0                  // Velocidad de elevación
Activation Radius: 3.0           // Radio de activación (si no es por tacto)
Player Tag: "Player"             // Tag del jugador
Require Player Touch: ✓          // Marcar = requiere contacto físico
                                 // Desmarcar = activación por proximidad
```

**Opciones de Activación**:

- **Por Contacto (Touch)** - RECOMENDADO:
  - `Require Player Touch: ✓ (marcado)`
  - El jugador debe tocar/colisionar con el Onion
  - Requiere Collider en el Onion (puede ser trigger o regular)

- **Por Proximidad**:
  - `Require Player Touch: ☐ (desmarcado)`
  - El jugador solo necesita estar cerca
  - Usa `Activation Radius` para definir la distancia

### Paso 6: Crear Puntos de Spawn y Recepción (Opcional)

1. **Crear Spawn Point** (donde emergen los Pikmin):
   - Clic derecho en `PikminOnion` → `Create Empty`
   - Nómbralo `SpawnPoint`
   - Muévelo a la posición deseada (ej: frente al Onion)
   - Arrastra este GameObject al campo `Spawn Point` del Onion

2. **Crear Pellet Receive Point** (donde llegan los pellets):
   - Clic derecho en `PikminOnion` → `Create Empty`
   - Nómbralo `PelletReceivePoint`
   - Muévelo a la posición deseada (ej: arriba del Onion)
   - Arrastra este GameObject al campo `Pellet Receive Point`

### Paso 7: Configuración de Pellets

#### **Pellet/Pillflower System**
```
Pellet Receive Point: (opcional) // Punto donde llegan los pellets
Pellet Absorb Radius: 2.0        // Radio de absorción automática
Pellet Absorb Speed: 5.0         // Velocidad de absorción
Pellet Absorb Effect: (opcional) // Partículas al absorber
```

### Paso 8: Auto Spawn (Opcional)

#### **Auto Spawn Settings**
```
Auto Spawn Enabled: ☐            // Marcar para auto-generación
Auto Spawn Interval: 5.0         // Segundos entre spawns automáticos
```

**Nota**: El auto-spawn solo funciona si:
- El Onion está ACTIVO (no enterrado)
- Hay Pikmin almacenados en el Onion
- El PikminManager puede aceptar más Pikmin

---

## Configuración del Sistema de Pellets

### Paso 1: Crear un Pellet Básico

1. **Crear GameObject**:
   - `Clic derecho → 3D Object → Cube` (o Cylinder para forma más realista)
   - Nómbralo `Pellet_1` (1 = valor del pellet)

2. **Ajustar tamaño**:
   - Scale: `(0.5, 0.5, 0.5)` o el tamaño que prefieras

3. **Añadir componentes necesarios**:
   ```
   - Rigidbody
   - Collider (Box/Sphere/Capsule)
   - Pellet script
   ```

4. **Configurar Rigidbody**:
   - Mass: `1.0`
   - Use Gravity: `✓ (marcado)`
   - Is Kinematic: `☐ (desmarcado)`

### Paso 2: Configurar el Script Pellet

#### **Pellet Properties**
```
Pikmin Value: 1                  // Cuántos Pikmin vale este pellet
Weight: 1.0                      // Peso (para sistema de carga)
Pellet Type: Number              // Tipo de pellet
Pellet Number: 1                 // Número mostrado (1, 5, 10, 20)
```

**Tipos de Pellet**:

1. **Number (Numerado)**:
   - Pellets estándar con números
   - Valor = número del pellet
   - Ejemplos: 1, 5, 10, 20

2. **Flower (Flor/Néctar)**:
   - Pellets de flores o néctar
   - Valor = 1 Pikmin normalmente

3. **Enemy (Enemigo)**:
   - Cadáveres de enemigos
   - Valor basado en el peso

#### **Carry Settings**
```
Can Be Carried: ✓                // Puede ser cargado
Is Being Carried: ☐              // (Se marca automáticamente)
Ready For Absorption: ☐          // (Se marca automáticamente)
```

#### **Visual Settings**
```
Pellet Color: Rojo               // Color del pellet
Pellet Renderer: (Auto)          // Renderer del objeto
Number Text: (opcional)          // TextMesh para mostrar número
```

#### **Physics**
```
Use Gravity: ✓                   // Usar gravedad
Onion Layer: Nothing             // (No necesario configurar)
```

### Paso 3: Crear Variantes de Pellets

#### **Pellet de 1**
```
Pikmin Value: 1
Pellet Number: 1
Pellet Color: Rojo claro
```

#### **Pellet de 5**
```
Pikmin Value: 5
Pellet Number: 5
Pellet Color: Rojo medio
Scale: (0.7, 0.7, 0.7)           // Más grande
```

#### **Pellet de 10**
```
Pikmin Value: 10
Pellet Number: 10
Pellet Color: Rojo oscuro
Scale: (0.9, 0.9, 0.9)           // Aún más grande
```

#### **Pellet de 20**
```
Pikmin Value: 20
Pellet Number: 20
Pellet Color: Rojo muy oscuro
Scale: (1.2, 1.2, 1.2)           // El más grande
```

### Paso 4: Añadir Texto de Número (Opcional)

1. **Crear TextMesh**:
   - Clic derecho en el Pellet → `3D Object → 3D Text`
   - Nómbralo `NumberText`

2. **Configurar TextMesh**:
   ```
   Text: "1" (o el número que corresponda)
   Font Size: 50
   Color: Blanco
   Anchor: Middle Center
   Alignment: Center
   ```

3. **Posicionar**:
   - Position: `(0, 0.3, 0)` (encima del pellet)
   - Rotation: `(90, 0, 0)` (mirando arriba)
   - Scale: `(0.1, 0.1, 0.1)`

4. **Asignar al script**:
   - Selecciona el Pellet
   - Arrastra el `NumberText` al campo `Number Text` del script Pellet

### Paso 5: Crear Prefabs de Pellets

1. **Crear carpeta**:
   - En `Assets`, crea una carpeta `Prefabs/Pellets`

2. **Crear prefabs**:
   - Arrastra cada pellet configurado a la carpeta
   - Nómbralos: `Pellet_1`, `Pellet_5`, `Pellet_10`, `Pellet_20`

3. **Eliminar de escena**:
   - Elimina las instancias de la escena (guarda solo los prefabs)

---

## Integración con el Jugador

### Paso 1: Configurar el Tag del Jugador

1. **Asignar tag**:
   - Selecciona tu GameObject de jugador
   - En el Inspector, en `Tag`, selecciona `Player`
   - Si no existe, créalo: `Add Tag → + → "Player"`

2. **Verificar Collider del jugador**:
   - Asegúrate de que el jugador tiene un Collider
   - Puede ser CharacterController, CapsuleCollider, etc.

### Paso 2: Configurar PikminManager (si existe)

1. **Ubicar PikminManager**:
   - Busca el GameObject con el script `PikminManager`
   - O créalo si no existe

2. **Verificar configuración**:
   ```
   Player Transform: (arrastra el jugador aquí)
   Max Pikmin: 100
   Formation Type: Circle
   ```

### Paso 3: Probar la Activación del Onion

1. **Iniciar el juego** (Play)
2. **Verificar posición inicial**:
   - El Onion debe estar bajo tierra si `Start Deactivated` está marcado
   - En la vista Scene, deberías ver gizmos mostrando las posiciones

3. **Acercarse al Onion**:
   - **Si Touch está activado**: Camina hacia el Onion hasta tocarlo
   - **Si Touch está desactivado**: Acércate dentro del radio de activación

4. **Observar la activación**:
   - El Onion debe emerger del suelo suavemente
   - Debe moverse desde la posición enterrada a la posición activa
   - Revisa la consola para mensajes de debug

### Paso 4: Probar el Spawn de Pikmin

**Desde el Inspector (durante Play Mode)**:

1. **Encontrar el Onion**:
   - Con el juego en ejecución, selecciona el PikminOnion

2. **En el Inspector**:
   - Busca los valores en tiempo real
   - Verifica `Current Pikmin Count` (debe ser > 0)

3. **Llamar método de spawn**:
   - Abre la consola (Window → General → Console)
   - En la ventana de Proyecto, crea un script temporal:

```csharp
// SpawnTester.cs - Script temporal para pruebas
using UnityEngine;

public class SpawnTester : MonoBehaviour
{
    public PikminOnion onion;

    void Update()
    {
        // Presiona la tecla P para spawner 1 Pikmin
        if (Input.GetKeyDown(KeyCode.P))
        {
            onion.RequestSpawnPikmin(1);
            Debug.Log("Spawn solicitado!");
        }

        // Presiona O para spawner 5 Pikmin
        if (Input.GetKeyDown(KeyCode.O))
        {
            onion.RequestSpawnPikmin(5);
            Debug.Log("Spawn de 5 solicitado!");
        }
    }
}
```

4. **Usar el tester**:
   - Añade este script al Onion
   - Arrastra el Onion al campo `Onion`
   - En Play mode, presiona `P` para spawner Pikmin

### Paso 5: Probar el Sistema de Pellets

1. **Colocar un pellet en la escena**:
   - Arrastra un prefab de Pellet a la escena
   - Posiciónalo cerca del Onion activado

2. **Marcar pellet como "siendo cargado"** (para prueba rápida):
   - Selecciona el Pellet durante Play mode
   - En el Inspector, marca `Is Being Carried`

3. **Mover el pellet cerca del Onion**:
   - Muévelo manualmente o usa código
   - Debe estar dentro del `Pellet Absorb Radius`

4. **Observar absorción**:
   - El pellet debe ser atraído hacia el Onion
   - Debe hacerse más pequeño
   - Debe desaparecer y añadir Pikmin al contador

---

## Efectos Visuales y Partículas

### Paso 1: Crear Sistema de Partículas para Spawn

1. **Crear Particle System**:
   - Clic derecho en jerarquía → `Effects → Particle System`
   - Nómbralo `SpawnEffect`

2. **Configurar partículas de spawn**:
   ```
   Duration: 1.0
   Looping: ☐ (desmarcado)
   Start Lifetime: 0.5
   Start Speed: 2
   Start Size: 0.2
   Start Color: Blanco/Amarillo
   Gravity Modifier: -0.5 (partículas suben)
   Emission → Rate over Time: 20
   Shape: Sphere, Radius: 0.5
   ```

3. **Crear Prefab**:
   - Arrastra `SpawnEffect` a la carpeta Prefabs
   - Elimina de la escena

4. **Asignar al Onion**:
   - Selecciona el PikminOnion
   - Arrastra el prefab al campo `Spawn Effect`

### Paso 2: Crear Efecto de Tierra/Excavación

1. **Crear Particle System**:
   - Crear nuevo: `Ground Dig Effect`

2. **Configurar partículas de tierra**:
   ```
   Duration: 1.5
   Start Lifetime: 1.0
   Start Speed: 3-5 (rango)
   Start Size: 0.1-0.3
   Start Color: Marrón/Tierra
   Gravity Modifier: 1.0
   Emission → Burst: Count 15-20
   Shape: Cone, Angle: 45
   Renderer → Material: Usar material marrón/tierra
   ```

3. **Crear Prefab y asignar**:
   - Crear prefab `GroundDigEffect`
   - Asignar al campo `Ground Dig Effect` del Onion

### Paso 3: Crear Efecto de Brillo del Onion

1. **Crear Particle System hijo**:
   - Clic derecho en PikminOnion → `Effects → Particle System`
   - Nómbralo `OnionGlow`

2. **Configurar brillo**:
   ```
   Duration: infinito (looping activado)
   Start Lifetime: 2.0
   Start Speed: 0.1
   Start Size: 0.5
   Start Color: Color del Onion (con alpha bajo)
   Emission: 5 partículas/segundo
   Shape: Sphere
   Color over Lifetime: Gradiente (opaco → transparente)
   ```

3. **Asignar**:
   - En el Onion, arrastra `OnionGlow` al campo `Onion Glow Effect`

### Paso 4: Crear Efecto de Activación

1. **Crear Particle System**:
   - Nuevo sistema: `ActivationEffect`

2. **Configurar activación**:
   ```
   Duration: 2.0
   Burst de partículas al inicio
   Start Speed: 5-8
   Color: Brillante, energético
   Shape: Sphere, Radius: 2
   Emission: Burst de 50 partículas
   ```

3. **Asignar al Onion**:
   - Campo `Activation Effect`

### Paso 5: Efecto de Absorción de Pellets

1. **Crear Particle System**:
   - Nuevo: `PelletAbsorbEffect`
   - Hacerlo hijo del PelletReceivePoint

2. **Configurar absorción**:
   ```
   Burst pequeño al absorber
   Color similar al pellet
   Partículas que implosionan hacia el centro
   Duration: 0.5
   ```

3. **Asignar**:
   - Campo `Pellet Absorb Effect` del Onion

---

## Pruebas y Depuración

### Modo Debug

1. **Activar mensajes de debug**:
   - En el Onion, marca `Show Debug Info`
   - En la consola verás mensajes detallados

2. **Activar Gizmos**:
   - Marca `Show Gizmos` en el Onion
   - En Scene view, activa Gizmos (botón en la parte superior)

### Visualización de Gizmos

Cuando seleccionas el Onion en el Editor, verás:

- **Esfera marrón**: Posición enterrada
- **Esfera cyan**: Posición activa
- **Línea vertical**: Conexión entre posiciones
- **Esfera amarilla**: Radio de activación (si proximity mode)
- **Esfera verde**: Radio de spawn de Pikmin
- **Esfera roja**: Posición subterránea de spawn de Pikmin
- **Esfera amarilla (otra)**: Radio de absorción de pellets

### Pruebas Paso a Paso

#### Test 1: Verificar Activación
```
1. Play mode
2. Observar que el Onion está bajo tierra
3. Acercarse al Onion
4. Verificar que emerge suavemente
5. Verificar mensaje en consola: "Onion is now ACTIVE!"
```

#### Test 2: Verificar Spawn de Pikmin
```
1. Activar el Onion primero
2. Asegurar Current Pikmin Count > 0
3. Presionar tecla de spawn (si tienes el script tester)
4. Observar Pikmin emergiendo del suelo
5. Verificar que se añaden al PikminManager
6. Verificar que siguen al jugador
```

#### Test 3: Verificar Absorción de Pellets
```
1. Colocar pellet en escena
2. En Play mode, seleccionar el pellet
3. Marcar "Is Being Carried"
4. Acercar el pellet al Onion
5. Observar que es absorbido
6. Verificar incremento en Current Pikmin Count
```

### Valores Recomendados para Diferentes Escenarios

#### Configuración Rápida (Prototipo)
```
Spawn Cooldown: 1.0
Dig Depth: 1.5
Rise Speed: 4.0
Buried Depth: 2.0
Emerge Speed: 5.0
```

#### Configuración Realista (Estilo Pikmin)
```
Spawn Cooldown: 2.5
Dig Depth: 2.0
Rise Speed: 1.5
Buried Depth: 3.5
Emerge Speed: 2.5
```

#### Configuración Cinematográfica
```
Spawn Cooldown: 3.0
Dig Depth: 2.5
Rise Speed: 0.8
Buried Depth: 5.0
Emerge Speed: 1.5
```

---

## Resolución de Problemas

### Problema: El Onion no se activa

**Posibles causas**:

1. **El jugador no tiene el tag "Player"**
   - Solución: Verificar que el GameObject del jugador tiene Tag = "Player"

2. **El Collider no está configurado correctamente**
   - Si `Require Player Touch = true`: El Onion necesita un Collider
   - Solución: Añadir Collider y marcar/desmarcar `Is Trigger` según prefieras

3. **El Onion ya está activo**
   - Solución: Verificar `Start Deactivated` está marcado

4. **El radio es muy pequeño** (modo proximity)
   - Solución: Aumentar `Activation Radius`

**Debug**:
```csharp
// Añadir esto temporalmente en CheckForPlayerActivation():
Debug.Log($"Player distance: {distance}, Activation radius: {activationRadius}");
```

### Problema: Los Pikmin no emergen del suelo

**Posibles causas**:

1. **El Onion no está activo**
   - Solución: Activar el Onion primero

2. **No hay Pikmin almacenados**
   - Solución: Aumentar `Current Pikmin Count` en el Inspector

3. **El prefab no está asignado**
   - Solución: Asignar el prefab de Pikmin al campo `Pikmin Prefab`

4. **Ground Layer no configurado**
   - Solución: Configurar `Ground Layer` correctamente

5. **El raycast no encuentra el suelo**
   - Solución: Verificar que hay geometría en la capa Ground debajo del spawn point

**Debug**:
```csharp
// En SpawnPikminFromGround(), antes del raycast:
Debug.DrawRay(spawnPosition + Vector3.up * 10f, Vector3.down * 50f, Color.red, 5f);
```

### Problema: Los Pikmin se quedan flotando o caen infinitamente

**Posibles causas**:

1. **El Rigidbody del Pikmin no está configurado**
   - Solución: Verificar que el prefab de Pikmin tiene Rigidbody con Use Gravity activado

2. **El Ground Layer no coincide**
   - Solución: Asegurar que el suelo está en la capa correcta

3. **El raycast no detecta el suelo**
   - Solución: Aumentar la distancia del raycast o revisar el LayerMask

### Problema: Los pellets no son absorbidos

**Posibles causas**:

1. **El pellet no está marcado como "Being Carried"**
   - Solución: Llamar `pellet.StartCarrying()` cuando los Pikmin lo carguen

2. **Está muy lejos del Onion**
   - Solución: Acercar el pellet o aumentar `Pellet Absorb Radius`

3. **El Onion no está activo**
   - Solución: Activar el Onion primero

4. **El Onion está lleno**
   - Solución: Verificar que Current Pikmin Count < Max Pikmin In Onion

**Debug**:
```csharp
// En el Pellet:
Debug.Log($"Ready for absorption: {readyForAbsorption}, Has been delivered: {hasBeenDelivered}");
```

### Problema: Errores de compilación

**Error: "PikminManager.Instance is null"**
- Solución: Asegurar que hay un PikminManager en la escena

**Error: "Pikmin prefab is null"**
- Solución: Asignar el prefab de Pikmin al Onion

**Error: "Ground layer not set"**
- Solución: Configurar el Ground Layer en el Inspector del Onion

### Problema: Rendimiento bajo con muchos Pikmin

**Optimizaciones**:

1. **Reducir Max Active Spawns**:
   - Cambiar de 5 a 2 o 3

2. **Aumentar Spawn Cooldown**:
   - De 2.0 a 3.0 o 4.0

3. **Desactivar efectos de partículas**:
   - Dejar los campos de efectos vacíos temporalmente

4. **Reducir Max Pikmin In Onion**:
   - De 50 a 20 o 30

---

## Scripts de Utilidad

### Script para Probar Spawning

Crea `Assets/Scripts/OnionTester.cs`:

```csharp
using UnityEngine;

public class OnionTester : MonoBehaviour
{
    [SerializeField] private PikminOnion onion;

    void Update()
    {
        // Presiona P para spawner 1 Pikmin
        if (Input.GetKeyDown(KeyCode.P))
        {
            onion.RequestSpawnPikmin(1);
        }

        // Presiona O para spawner 5 Pikmin
        if (Input.GetKeyDown(KeyCode.O))
        {
            onion.RequestSpawnPikmin(5);
        }

        // Presiona I para añadir 10 Pikmin al almacenamiento
        if (Input.GetKeyDown(KeyCode.I))
        {
            onion.AddPikminToStorage(10);
        }

        // Presiona A para activar el Onion manualmente
        if (Input.GetKeyDown(KeyCode.A))
        {
            onion.ActivateOnion();
        }

        // Mostrar info en pantalla
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log($"Estado: {onion.GetCurrentState()}");
            Debug.Log($"Pikmin almacenados: {onion.GetStoredPikminCount()}");
            Debug.Log($"En cola de spawn: {onion.GetQueuedSpawnCount()}");
            Debug.Log($"Está activo: {onion.IsActive()}");
        }
    }
}
```

### Script para Simular Carga de Pellets

Crea `Assets/Scripts/PelletCarrier.cs`:

```csharp
using UnityEngine;

public class PelletCarrier : MonoBehaviour
{
    [SerializeField] private float carrySpeed = 2f;
    [SerializeField] private Transform targetOnion;
    private Pellet pellet;

    void Start()
    {
        pellet = GetComponent<Pellet>();
        if (pellet != null)
        {
            pellet.StartCarrying();
        }

        if (targetOnion == null)
        {
            PikminOnion onion = FindObjectOfType<PikminOnion>();
            if (onion != null)
                targetOnion = onion.transform;
        }
    }

    void Update()
    {
        if (targetOnion != null && pellet != null && pellet.IsBeingCarried())
        {
            // Mover hacia el Onion
            Vector3 direction = (targetOnion.position - transform.position).normalized;
            transform.position += direction * carrySpeed * Time.deltaTime;
        }
    }
}
```

Añade este script a un pellet para que se mueva automáticamente hacia el Onion.

---

## Consejos Finales

### Mejores Prácticas

1. **Siempre probar con valores de debug primero**:
   - `Current Pikmin Count = 20` (para tener suficientes para probar)
   - `Show Debug Info = true`
   - `Show Gizmos = true`

2. **Configurar capas desde el inicio**:
   - Ground layer para el suelo
   - Obstacle layer para obstáculos (si usas en Pikmin.cs)

3. **Usar prefabs**:
   - Mantén todos los Pikmin y Pellets como prefabs
   - Facilita hacer cambios globales

4. **Organizar la jerarquía**:
   ```
   GameWorld
   ├── Environment
   │   └── Ground (Layer: Ground)
   ├── Onions
   │   └── PikminOnion
   │       ├── Model
   │       ├── SpawnPoint
   │       └── PelletReceivePoint
   ├── Pellets
   │   ├── Pellet_1
   │   └── Pellet_5
   └── Player
   ```

5. **Backup antes de cambios grandes**:
   - Duplica la escena antes de modificar configuraciones importantes

### Checklist de Configuración Completa

- [ ] PikminOnion GameObject creado
- [ ] Script PikminOnion añadido
- [ ] Collider añadido al Onion
- [ ] Ground Layer creada y asignada
- [ ] Prefab de Pikmin asignado
- [ ] Player tiene tag "Player"
- [ ] Valores de spawning configurados
- [ ] Valores de activación configurados
- [ ] Pellets creados con script
- [ ] Pellets tienen Rigidbody y Collider
- [ ] Efectos de partículas creados (opcional)
- [ ] Sistema probado en Play mode
- [ ] Debug info verificado en consola

---

## Recursos Adicionales

### Archivos del Sistema
- `Assets/Scripts/PikminOnion.cs` - Script principal del Onion
- `Assets/Scripts/Pellet.cs` - Script de pellets
- `Assets/Scripts/Pikmin.cs` - Script de Pikmin individual
- `Assets/Scripts/PikminManager.cs` - Gestor de Pikmin

### Contacto y Soporte
Si encuentras problemas no cubiertos en esta guía:
1. Revisa la consola para mensajes de error
2. Verifica que todos los pasos se siguieron correctamente
3. Usa los scripts de utilidad para debugging
4. Revisa los gizmos en Scene view

---

**¡Buena suerte con tu proyecto de Pikmin!**
