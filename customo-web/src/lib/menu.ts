export type Opt = { id: number; type: "Size" | "Extra" | "Swap"; name: string; delta: number };
export type Ingredient = { id: number; name: string; allergen: string | null; removable: boolean };
export type Dish = { id: number; name: string; cat: string; price: number; allergens: string[]; ingredients: Ingredient[]; options: Opt[] };
export const CATS = ["All", "Mains", "Sides", "Desserts", "Drinks"];
export const ALLERGENS = ["Peanuts", "Tree nuts", "Milk", "Egg", "Wheat", "Soy", "Fish", "Shrimp", "Shellfish", "Sesame"];
