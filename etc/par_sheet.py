import re

def build_combo_table(reels: list[list[str]]):

    
    combo_table = reels[0]
    for reel in reels[1:]:
        stack = []
        for combo0 in combo_table:
            for symbol in reel:
                stack.append(combo0 + symbol)

        combo_table = stack
    return combo_table
            
def calculate_odds(pay_sheet: dict[str, int], combo_table: list[str]) -> float:

    wins = 0
    for key, value in pay_sheet.items():
        regex = re.compile(key)
        for combo in combo_table:
            if re.match(regex, combo):
                wins += value

    return float(wins) / len(combo_table)

if __name__ == "__main__":
    reel1 = ["1", "1", "1", "1", "2", "2", "2", "3", "3", "4", "4", "5"]
    reel2 = ["1", "1", "1", "2", "2", "3", "3", "3", "3", "4",  "4", "4", "5"]
    reel3 = ["1", "1", "1", "2", "2", "3", "3", "4", "4", "4", "5"]

    pay_sheet = {
        "5..": 1,
        ".5.": 1,
        "..5": 1,
        "55.": 2,
        "5.5": 2,
        ".55": 2,
        "111": 2,
        "222": 4,
        "333": 8,
        "444": 16,
        "555": 100,
    }
    # Add one to represent bet amount return
    for key, value in pay_sheet.items():
        pay_sheet[key] = value + 1
    
    combo_table = build_combo_table([reel1, reel2, reel3])

    odds = calculate_odds(pay_sheet, combo_table)
    print(odds)
    


