**Title: Introducing `sharpurs`: A PureScript to .NET Backend using an "FFI Squared" Architecture**

Dear all,

I’ve really enjoyed building custom backends for PureScript recently, and I’m using my experience to keep pushing the boundaries. Now that [`phpurs`](https://github.com/...) is running in production at my company, and [`gopurs`](https://github.com/...) has broken through the sound barrier thanks to our TAST-based compiler fork, I’d like to introduce another backend currently in development: **`sharpurs`**.

My apologies for the multiple messages recently. Even though no one has complained about it, I realize that it might be a bit much. My intentions are genuine, and I sincerely want to demonstrate the natural universality of PureScript. I'm riding the current **momentum**, while all these ideas around compilation are still fresh in my mind, to give as many fruits as possible back to the community. I'll probably publish fewer "devlog" messages, to concentrate them around official releases. In fact, `gopurs` is now heading towards its official release (even though a few subjects remain to be tackled), which naturally leaves room for `sharpurs` as a new early-stage WIP project.

The core idea behind `sharpurs` is simple but incredibly powerful: create an FFI^2 architecture to leverage the massive C#/.NET ecosystem, while using F# to drastically simplify the compilation process.

### Why target F# instead of C# directly?
While the ultimate goal is to run on the .NET CLR and consume C# libraries (NuGet, BCL), compiling a pure functional language directly to an Object-Oriented one like C# requires generating a lot of boilerplate to simulate functional concepts. 
By targeting F# (an ML-family language) as our intermediate step, the translation is incredibly natural: 
* PureScript ADTs map 1:1 to F# Discriminated Unions.
* Currying, immutability, and exhaustive pattern matching are natively supported and optimized by the F# compiler.

We’re seeing very impressive performance for these natively optimized functional concepts.

### The "FFI Squared" Architecture (`.purs` -> `.fs` -> `.cs`)
The most exciting part is how Foreign Function Interfaces (FFI) are handled. It acts as a perfect funnel:

1. **`Module.purs`**: You declare your mathematically pure functions using `foreign import`.
2. **`Module.fs`**: You write your PureScript FFI files in F#. This acts as the elegant bridge. 
3. **`Module.cs` / NuGet**: Because F# and C# share the exact same runtime (CLR), your F# FFI file can natively `open` C# namespaces, instantiate C# objects, and call C# methods with zero overhead or serialization penalties. 

This architecture gives you complete freedom when implementing your FFIs: you can write pure F# logic directly in `.fs` files, you can write raw C# code in companion `.cs` files, or you can import any existing C# NuGet package. Both F# and C# interoperate seamlessly, allowing you to choose the language that best fits the problem at hand.

In the end, the `.NET` compiler gathers your `.fs` and `.cs` files, links your NuGet packages, and builds a single, highly optimized binary.

Essentially, `sharpurs` allows us to enforce the mathematical purity and safety of PureScript at the top, while flawlessly driving the industrial muscle of the C# ecosystem at the bottom. 

The early results are very promising.

Stay tuned.
