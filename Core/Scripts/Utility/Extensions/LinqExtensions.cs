using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

// ReSharper disable PossibleMultipleEnumeration

// Suppress warning about undocumented parameters. Extension methods shouldn't have to document this.
#pragma warning disable 1573

namespace DigitalSalmon.Extensions
{
    public static class LinqExtensions
    {
        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        private static readonly Random shuffleRandom         = new Random();
        private static readonly Random weightedElementRandom = new Random();

        public static Quaternion QuatMul(this IEnumerable<Quaternion> self)
        {
            Quaternion result = Quaternion.identity;
            foreach (Quaternion quat in self)
            {
                result *= quat;
            }

            return result;
        }

        public static Quaternion QuatMul<TSource>(this IEnumerable<TSource> source, Func<TSource, Quaternion> selector) => source.Select(selector).QuatMul();

        public static Vector2 WeightedAverage<T>(this IEnumerable<T> records, Func<T, Vector2> value, Func<T, float> weight)
        {
            Vector2 weightedValueSum = new Vector2(records.Sum(x => value(x).x * weight(x)), records.Sum(x => value(x).y * weight(x)));
            float weightSum = records.Sum(weight);
            return weightedValueSum / weightSum;
        }

        public static float WeightedAverage<T>(this IEnumerable<T> records, Func<T, float> value, Func<T, float> weight)
        {
            float weightedValueSum = records.Sum(x => value(x) * weight(x));
            float weightSum = records.Sum(weight);
            return weightedValueSum / weightSum;
        }

        /// <summary>
        /// Adds values from 'other' to this collection if they meet a given predicate.
        /// Calls 'onAdd' on each element that is added, after adding.
        /// </summary>
        public static void AddWhere<T>(this ICollection<T> self, ICollection<T> other, Func<T, bool> predicate, Action<T> onAdd = null)
        {
            foreach (T value in other)
            {
                if (predicate(value))
                {
                    self.Add(value);
                    onAdd?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Removes values from 'other' from this collection if they meet a given predicate.
        /// Calls 'onRemove' on each element that is removed, before removal.
        /// </summary>
        public static void RemoveWhere<T>(this ICollection<T> self, Func<T, bool> predicate, Action<T> onRemove = null)
        {
            foreach (T value in self.Where(predicate).ToList())
            {
                onRemove?.Invoke(value);
                self.Remove(value);
            }
        }

        public static (TSource element, TKey maxValue) MaxElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) => source.MaxElement(selector, null);

        public static (TSource element, TKey maxValue) MaxElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            comparer = comparer ?? Comparer<TKey>.Default;

            using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }

                TSource max = sourceIterator.Current;
                TKey maxKey = selector(max);
                while (sourceIterator.MoveNext())
                {
                    TSource candidate = sourceIterator.Current;
                    TKey candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }

                return (max, maxKey);
            }
        }

        public static (TSource element, TKey minValue) MinElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) => source.MinElement(selector, null);

        public static (TSource element, TKey minValue) MinElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            comparer = comparer ?? Comparer<TKey>.Default;

            using (IEnumerator<TSource> sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }

                TSource min = sourceIterator.Current;
                TKey minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    TSource candidate = sourceIterator.Current;
                    TKey candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }

                return (min, minKey);
            }
        }

        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector) => RandomElementByWeight(sequence, weightSelector, weightedElementRandom);

        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector, Random random)
        {
            float totalWeight = sequence.Sum(weightSelector);

            float itemWeightIndex = totalWeight * (float) random.NextDouble();

            float currentWeightIndex = 0;

            foreach (var item in sequence.Select(e => new {Value = e, Weight = weightSelector(e)}))
            {
                currentWeightIndex += item.Weight;

                // If we've hit or passed the weight we are after for this item then it's the one we want....
                if (currentWeightIndex >= itemWeightIndex) return item.Value;
            }

            return default;
        }

        /// <summary>
        /// Adds the element if the element is not already contained within the List.
        /// </summary>
        public static void AddIfUnique<T>(this List<T> self, T element)
        {
            if (!self.Contains(element)) self.Add(element);
        }

        /// <summary>
        /// Adds the element if the element is not already contained within the Hashset.
        /// </summary>
        public static void AddIfUnique<T>(this HashSet<T> self, T element)
        {
            if (!self.Contains(element)) self.Add(element);
        }

        /// <summary>
        /// Shuffle the elements in the <c>IList</c> using Fisher-Yates select and swap.
        /// </summary>
        public static void Shuffle<T>(this IList<T> self) { self.Shuffle(shuffleRandom); }

        /// <summary>
        /// Shuffle the elements in the <c>IList</c> using Fisher-Yates select and swap.
        /// </summary>
        public static void Shuffle<T>(this IList<T> self, Random random)
        {
            int n = self.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = self[k];
                self[k] = self[n];
                self[n] = value;
            }
        }

        /// <summary>
        /// Returns the index of a randomly selected element in the IEnuemrable.
        /// </summary>
        public static int RandomElementIndex<T>(this IEnumerable<T> self)
        {
            int index = UnityEngine.Random.Range(0, self.Count());
            return index;
        }

        /// <summary>
        /// Returns a random element from an <c>IEnumerable</c>. Special case for 'int' type.
        /// Note: RandomOrDefault is much faster when used on a collection of 'int' or 'float'.
        /// Count &lt; 100 optimised from
        /// https://nickstips.wordpress.com/2010/08/28/c-optimized-extension-method-get-a-random-element-from-a-collection/
        /// </summary>
        public static int RandomOrDefault(this IEnumerable<int> self, Random random = null)
        {
            if (random == null) random = new Random();
            int count = self.Count();
            int index = random.Next(count);

            if (self is IList<int> list) return list[index];

            // When the collection has 100 elements or less, get the random element
            // by traversing the collection one element at a time.
            if (count > 100) return self.ElementAt(index);

            using (IEnumerator<int> enumerator = self.GetEnumerator())
            {
                while (index >= 0 && enumerator.MoveNext()) index--;
                return enumerator.Current;
            }
        }

        /// <summary>
        /// Returns a random element from an <c>IEnumerable</c>. Special case for 'float' type.
        /// Note: RandomOrDefault is much faster when used on a collection of 'int' or 'float'.
        /// /// Count &lt; 100 optimised from
        /// https://nickstips.wordpress.com/2010/08/28/c-optimized-extension-method-get-a-random-element-from-a-collection/
        /// </summary>
        public static float RandomOrDefault(this IEnumerable<float> self, Random random = null)
        {
            if (random == null) random = new Random();
            int count = self.Count();
            int index = random.Next(count);

            if (self is IList<float> list) return list[index];

            // When the collection has 100 elements or less, get the random element
            // by traversing the collection one element at a time.
            if (count > 100) return self.ElementAt(index);

            using (IEnumerator<float> enumerator = self.GetEnumerator())
            {
                while (index >= 0 && enumerator.MoveNext()) index--;
                return enumerator.Current;
            }
        }

        /// <summary>
        /// Returns a random element from an <c>IEnumerable</c>.
        /// </summary>
        public static T RandomOrDefault<T>(this IEnumerable<T> self, Random random = null)
        {
            if (random == null) random = new Random();
            int count = self.Count();
            int index = random.Next(count);
            if (self is IList<T> list) return list[index];

            // When the collection has 100 elements or less, get the random element
            // by traversing the collection one element at a time.
            if (count > 100) return self.ElementAt(index);

            using (IEnumerator<T> enumerator = self.GetEnumerator())
            {
                while (index >= 0 && enumerator.MoveNext()) index--;
                return enumerator.Current;
            }
        }

        /// <summary>
        /// Returns a random element from an <c>IEnumerable</c> which satisfies the given predicate.
        /// </summary>
        /// <param name="self">The <c>IEnumerable</c>.</param>
        /// <param name="predicate">A predicate which each element must satisfy.</param>
        public static T RandomOrDefault<T>(this IEnumerable<T> self, Func<T, bool> predicate, Random random = null)
        {
            if (random == null) random = new Random();
            T randomElement = default;
            int count = 0;
            foreach (T element in self)
            {
                if (!predicate(element)) continue;
                ++count;
                if (random.NextDouble() <= 1f / count)
                {
                    randomElement = element;
                }
            }

            return randomElement;
        }

        /// <summary>
        /// Moves all elements which match 'Predicate' by 'offset'.
        /// </summary>
        /// <param name="itemSelector">Predicate for element selection</param>
        /// <param name="offset">Selected elements local offset. Positive and Negative supported.</param>
        public static void Move<T>(this List<T> self, Predicate<T> itemSelector, int offset)
        {
            List<T> locatedItems = self.FindAll(itemSelector);
            foreach (T item in locatedItems)
            {
                int currentIndex = self.IndexOf(item);
                int nextIndex = currentIndex + offset;

                if (nextIndex > self.Count - 1 || nextIndex < 0 || currentIndex == -1) return;

                // Copy the current item
                T currentItem = self[currentIndex];

                // Remove the item
                self.RemoveAt(currentIndex);

                // Finally add the item at the new index
                self.Insert(nextIndex, currentItem);
            }
        }

        /// <summary>
        /// Calls an action on each item before yielding them.
        /// </summary>
        /// <param name="action">The action to call for each item.</param>
        public static IEnumerable<T> Examine<T>(this IEnumerable<T> self, Action<T> action, float dog = 0)
        {
            foreach (T item in self)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// Perform an action on each item.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (T item in self)
            {
                action(item);
            }

            return self;
        }

        /// <summary>
        /// Perform an action on each item.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> self, Action<T, int> action)
        {
            int counter = 0;

            foreach (T item in self)
            {
                action(item, counter++);
            }

            return self;
        }

        /// <summary>
        /// Convert each item in the collection.
        /// </summary>
        /// <param name="converter">Func to convert the items.</param>
        public static IEnumerable<T> Convert<T>(this IEnumerable self, Func<object, T> converter)
        {
            foreach (object item in self)
            {
                yield return converter(item);
            }
        }

        /// <summary>
        /// Convert a colletion to a HashSet.
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> self) => new HashSet<T>(self);

        /// <summary>
        /// Add an item to the beginning of a collection.
        /// </summary>
        /// <param name="self">The collection.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> self, Func<T> prepend)
        {
            yield return prepend();

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of a collection.
        /// </summary>
        /// <param name="prepend">The item to prepend.</param>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> self, T prepend)
        {
            yield return prepend;

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection.
        /// </summary>
        /// <param name="prepend">The collection to prepend.</param>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> self, IEnumerable<T> prepend)
        {
            foreach (T item in prepend)
            {
                yield return item;
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> self, bool condition, Func<T> prepend)
        {
            if (condition)
            {
                yield return prepend();
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> self, bool condition, T prepend)
        {
            if (condition)
            {
                yield return prepend;
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The collection to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> self, bool condition, IEnumerable<T> prepend)
        {
            if (condition)
            {
                foreach (T item in prepend)
                {
                    yield return item;
                }
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> self, Func<bool> condition, Func<T> prepend)
        {
            if (condition())
            {
                yield return prepend();
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> self, Func<bool> condition, T prepend)
        {
            if (condition())
            {
                yield return prepend;
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The collection to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> self, Func<bool> condition, IEnumerable<T> prepend)
        {
            if (condition())
            {
                foreach (T item in prepend)
                {
                    yield return item;
                }
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> self, Func<IEnumerable<T>, bool> condition, Func<T> prepend)
        {
            if (condition(self))
            {
                yield return prepend();
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The item to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> self, Func<IEnumerable<T>, bool> condition, T prepend)
        {
            if (condition(self))
            {
                yield return prepend;
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The collection to prepend.</param>
        public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> self, Func<IEnumerable<T>, bool> condition, IEnumerable<T> prepend)
        {
            if (condition(self))
            {
                foreach (T item in prepend)
                {
                    yield return item;
                }
            }

            foreach (T item in self)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Add an item to the end of a collection.
        /// </summary>
        /// >
        /// <param name="append">Func to create the item to append.</param>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> self, Func<T> append)
        {
            foreach (T item in self)
            {
                yield return item;
            }

            yield return append();
        }

//		/// <summary>
//		/// Add an item to the end of a collection.
//		/// </summary>
//		/// <param name="append">The item to append.</param>
//		public static IEnumerable<T> Append<T>(this IEnumerable<T> self, T append) {
//			foreach (T item in self) {
//				yield return item;
//			}
//
//			yield return append;
//		}

/// <summary>
/// Add a collection to the end of another collection.
/// </summary>
/// <param name="append">The collection to append.</param>
public static IEnumerable<T> Append<T>(this IEnumerable<T> self, IEnumerable<T> append)
        {
            foreach (T item in self)
            {
                yield return item;
            }

            foreach (T item in append)
            {
                yield return item;
            }
        }

/// <summary>
/// Add an item to the end of a collection if a condition is met.
/// </summary>
/// <param name="condition">The condition.</param>
/// <param name="append">Func to create the item to append.</param>
public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> self, bool condition, Func<T> append)
        {
            foreach (T item in self)
            {
                yield return item;
            }

            if (condition)
            {
                yield return append();
            }
        }

/// <summary>
/// Add an item to the end of a collection if a condition is met.
/// </summary>
/// <param name="condition">The condition.</param>
/// <param name="append">The item to append.</param>
public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> self, bool condition, T append)
        {
            foreach (T item in self)
            {
                yield return item;
            }

            if (condition)
            {
                yield return append;
            }
        }

/// <summary>
/// Add a collection to the end of another collection if a condition is met.
/// </summary>
/// <param name="condition">The condition.</param>
/// <param name="append">The collection to append.</param>
public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> self, bool condition, IEnumerable<T> append)
        {
            foreach (T item in self)
            {
                yield return item;
            }

            if (!condition) yield break;
            {
                foreach (T item in append)
                {
                    yield return item;
                }
            }
        }

/// <summary>
/// Add an item to the end of a collection if a condition is met.
/// </summary>
/// <param name="condition">The condition.</param>
/// <param name="append">Func to create the item to append.</param>
public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> self, Func<bool> condition, Func<T> append)
        {
            foreach (T item in self)
            {
                yield return item;
            }

            if (condition())
            {
                yield return append();
            }
        }

/// <summary>
/// Add an item to the end of a collection if a condition is met.
/// </summary>
/// <param name="condition">The condition.</param>
/// <param name="append">The item to append.</param>
public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> self, Func<bool> condition, T append)
        {
            foreach (T item in self)
            {
                yield return item;
            }

            if (condition())
            {
                yield return append;
            }
        }

/// <summary>
/// Add a collection to the end of another collection if a condition is met.
/// </summary>
/// <param name="condition">The condition.</param>
/// <param name="append">The collection to append.</param>
public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> self, Func<bool> condition, IEnumerable<T> append)
        {
            foreach (T item in self)
            {
                yield return item;
            }

            if (!condition()) yield break;
            {
                foreach (T item in append)
                {
                    yield return item;
                }
            }
        }

/// <summary>
/// Returns and casts only the items of type <typeparamref name="T" />.
/// </summary>
public static IEnumerable<T> FilterCast<T>(this IEnumerable self)
        {
            foreach (object obj in self)
            {
                if (obj is T variable)
                {
                    yield return variable;
                }
            }
        }

/// <summary>
/// Adds a collection to a hashset.
/// </summary>
/// <param name="range">The collection.</param>
public static void AddRange<T>(this HashSet<T> self, IEnumerable<T> range)
        {
            foreach (T value in range)
            {
                self.Add(value);
            }
        }

/// <summary>
/// Returns <c>true</c> if the list is either null or empty. Otherwise <c>false</c>.
/// </summary>
public static bool IsNullOrEmpty<T>(this IList<T> self) => self == null || self.Count == 0;

/// <summary>
/// Sets all items in the list to the given value.
/// </summary>
/// <param name="item">The value.</param>
public static void Populate<T>(this IList<T> self, T item)
        {
            int count = self.Count;
            for (int i = 0; i < count; i++)
            {
                self[i] = item;
            }
        }

/// <summary>
/// Take all elements until predicate is true, including the first element where predicate is true.
/// </summary>
public static IEnumerable<T> TakeUntilIncluding<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            foreach (T el in list)
            {
                yield return el;
                if (predicate(el)) yield break;
            }
        }

/// <summary>
/// Take all elements until predicate is true
/// </summary>
public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            foreach (T el in list)
            {
                if (predicate(el)) yield break;
                yield return el;
            }
        }
    }
}