# Is insider buying always an indication of a soon-to-be rising stock price?
Peter Lynch, the legendary former manager of Fidelity's Magellan fund once said "Insiders might sell their shares for any number of reasons, but they buy them for only one: they think the price will rise". In the years since Lynch made this statement, it has become a common maxim that insider buying (when an officer, director, or major shareholder) is an indication that there is something brewing at the company that will soon be followed with abnormal performance of the stock price.  
But is this always the case? Is every insider purchase followed by a gain? By how much do these gains eclipse that of the stock's usual performance? Is it possible that in some cases this is nothing more than a C-level officer trying to instill the public's confidence in the stock by putting his own money on the line? In this article I will attempt to answer these questions with candid research.

## The Analysis
To answer these questions we will assess the performance of S&P 500 components following insider purchases made between 2010 through 2019. For each insider purchase, we will measure the performance of the stock at the following intervals after the purchase date:

- 14 days
- 30 days
- 90 days
- 180 days
- 360 days

All of these post-purchase performances will then be averaged together to establish the average performance of this stock following an insider purchase. This performance set will then be compared to the stock's average performance (for these same intervals) over the course of 2010 through 2019.  
By comparing the stock's average performance in ten years (or since IPO) to the stock's immediate performance following an insider purchase, we can gain insight into the correlation between stock price and insider buying.

## Conducting the Analysis
We will be Aletheia (https://aletheiaapi.com/) for accessing insider trading data for the S&P 500. Aletheia has a terrific endpoint that provides insider trading history for any publicly traded company: https://aletheiaapi.com/api/#latest-transactions. We can query insider transactions for a particular company and filter the results to only equity-based security purchases that occured between 2010-2019.