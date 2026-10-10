# Dependency Injection en .NET

## Dependencias explícitas

Una dependencia es una pieza que otra necesita para realizar su trabajo. Un servicio que envía mensajes a un modelo necesita un cliente de chat. Si recibe ese cliente desde afuera, puede concentrarse en su responsabilidad principal. La creación del cliente queda en otro lugar y resulta más fácil reconocer qué requiere el servicio para funcionar.

Dependency Injection, o DI, es una forma de proporcionar esas dependencias. El constructor expresa lo que necesita la clase y el código de composición decide qué objetos entregar. No hace falta introducir un contenedor para comprender la idea: crear un cliente y pasarlo a un constructor ya permite observar la separación entre construir una dependencia y utilizarla.

## La responsabilidad de componer

Componer una aplicación significa conectar objetos que tienen responsabilidades distintas. Una clase puede encargarse de coordinar una operación y otra de acceder a una capacidad externa. Mantener visible esa relación facilita leer el programa y decidir dónde debe vivir cada cambio. La clase consumidora no necesita conocer todos los detalles de construcción de sus colaboradores.

El beneficio aparece cuando existe una responsabilidad que vale la pena separar. Una interfaz o un contenedor no deben agregarse solamente para demostrar un patrón. En un ejercicio corto, la construcción directa puede ser suficiente. Cuando el flujo crece o queremos practicar la composición de servicios, DI ofrece un vocabulario conocido para expresar esa organización.

## ServiceCollection

En los ejemplos con contenedor, `ServiceCollection` reúne registros que describen cómo obtener las dependencias. Un registro puede asociar una interfaz con una implementación concreta o utilizar una factory para construir una instancia. Registrar todavía no significa ejecutar todo el programa: estamos declarando cómo se compondrán los objetos cuando sean solicitados.

Después de registrar las piezas, `BuildServiceProvider` construye un proveedor capaz de resolverlas. El código de entrada puede solicitar el servicio principal mediante `GetRequiredService`. Si una clase recibe otras dependencias en su constructor, el contenedor utiliza los registros disponibles para obtenerlas. Conviene mantener esta composición cerca del punto de entrada de la aplicación.

## Inyección por constructor

La inyección por constructor hace visibles los requisitos de una clase. Por ejemplo, un servicio que coordina consultas puede recibir un cliente de chat y otras piezas necesarias para su tarea. El constructor las conserva en campos y los métodos las utilizan. La clase evita construir clientes concretos durante cada operación, lo que permite revisar su responsabilidad sin mezclarla con configuración.

Si el servicio necesita cargar variables de entorno, elegir un proveedor y construir todos sus clientes internamente, esas tareas se acumulan con la operación principal. Separarlas permite que la configuración validada llegue a las factories y que el servicio reciba contratos ya construidos. El punto no es reducir líneas, sino mantener clara la relación entre preparación y ejecución.

## Registro y resolución

Un registro conecta el contrato que se solicita con la forma de obtener un objeto. La resolución utiliza ese registro. Si falta una dependencia requerida, la composición no puede completarse. Por eso resulta útil revisar los constructores junto con los registros: deben expresar una relación coherente entre lo que cada clase necesita y lo que la aplicación puede proporcionar.

El consumidor no debería recorrer el contenedor para buscar sus propias dependencias en cada método. Recibirlas por constructor mantiene esos requisitos visibles. El programa principal puede resolver el servicio inicial y dejar que la composición haga el resto. Para un laboratorio pequeño, basta con observar unos pocos registros y un objeto principal para comprender el mecanismo.

## Singleton

Un registro Singleton permite reutilizar una instancia dentro del mismo proveedor de servicios. Esto puede ser apropiado para una pieza cuyo estado debe compartirse durante el proceso. Por ejemplo, si un índice en memoria contiene datos cargados al inicio, los consumidores deben recibir la instancia que conserva esos datos, en lugar de obtener índices vacíos diferentes.

Singleton no significa almacenamiento persistente ni una instancia universal fuera de la aplicación. El alcance está relacionado con el proveedor que creamos. Al finalizar el proceso se pierde el estado que solo vivía en RAM. Cuando una pieza mutable se comparte entre consumidores, también hay que comprender quién la modifica y bajo qué condiciones se utiliza.

## Transient

Un registro Transient crea una instancia nueva cada vez que se solicita esa dependencia al contenedor. Puede utilizarse para un servicio que coordina operaciones sin conservar estado propio entre resoluciones. Esa instancia puede recibir una dependencia Singleton y trabajar sobre el mismo estado compartido que utilizan otros consumidores del proveedor.

La diferencia entre ambos ciclos de vida resulta observable cuando resolvemos dos servicios y analizamos qué objetos comparten. Dos coordinadores pueden ser distintos y aun así utilizar el mismo índice. Elegir el ciclo de vida debe responder al comportamiento esperado del programa. La elección deja de ser una etiqueta cuando podemos explicar qué debe conservarse y qué puede reconstruirse.

## DI y simplicidad

DI organiza la construcción y entrega de dependencias; no agrega automáticamente capacidades de inteligencia artificial. Un servicio sigue necesitando un algoritmo, datos e instrucciones para realizar su trabajo. El contenedor conecta piezas, pero no decide el contenido de una respuesta ni reemplaza el flujo que queremos enseñar en un laboratorio.

Por eso el recorrido utiliza DI cuando existe una razón pedagógica concreta y conserva construcción directa cuando ayuda a observar otro concepto. Conocer una herramienta no obliga a incorporarla en cada ejemplo. La composición adecuada es la que permite entender qué objetos colaboran, por qué lo hacen y qué responsabilidad mantiene cada uno dentro de la aplicación.
