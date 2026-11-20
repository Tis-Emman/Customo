export type Opt = { id: number; type: "Size" | "Extra" | "Swap"; name: string; delta: number };
// available = false: sold out right now (the kitchen marked it, or the stock ran out)
export type Ingredient = { id: number; name: string; allergen: string | null; removable: boolean; available: boolean };
export type Dish = { id: number; name: string; cat: string; price: number; allergens: string[]; ingredients: Ingredient[]; options: Opt[]; available: boolean };
export const CATS = ["All", "Mains", "Sides", "Desserts", "Drinks"];
export const ALLERGENS = ["Peanuts", "Tree nuts", "Milk", "Egg", "Wheat", "Soy", "Fish", "Shrimp", "Shellfish", "Sesame"];

// Removable ingredients that are sold out: they are left out of the dish automatically
export const soldOutExtras = (d: Dish) => d.ingredients.filter((i) => i.removable && !i.available);
