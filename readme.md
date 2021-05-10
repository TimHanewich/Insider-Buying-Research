# Does insider buying always result in stock price gains?
We will be using the Aletheia API to study the correlation between trends of insider buying and stock performance. Does insider buying always mean the insiders sure the price will rise, or is this false confidence?

This is the procedure that will be followed:
- For all insider trades between 2010 and 2020, search for notable net upticks in insider buying
    - A "notable uptick" is defined as the net (buys against sells) trailing 45-day insider trading surpasses the mean trailing 45 day insider trading by 50%.
    - The "timer" will start on the last day of this - in other words, when the round of buying has concluded. So the last day in which the threshold has been passed.
- The stock price on that day will be taken. The stock price will then be evaluated at 
    - 30 days
    - 90 days
    - 180 days
    - 360 days (1 year)
- The stock price changes will be compared.