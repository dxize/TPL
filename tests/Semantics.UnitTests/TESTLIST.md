# Список тестов Семантического анализа

## Корректные программы
```
func int main() { return 0; }
func int main() { print(42); return 0; }
func int main() { print(3.14); return 0; }
func int main() { print("hello"); return 0; }
func int main() { print(1, 2.5, "dea"); return 0; }
func int main() { print(1); print(2); return 0; }
```
## Ошибки main
- [x] Пустое тело: `func int main() {}`
- [x] Отсутствие return: `func int main() { print(1); }`
- [x] Инструкция после return: `func int main() { return 0; print(1); }`