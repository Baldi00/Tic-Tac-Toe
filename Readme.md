# TIC-TAC-TOE

Hello candidate! Thanks for your time and effort in taking this test.
This is a simple testing task aiming to test different of your skills as a Unity programmer.
You will need to implement a [TIC-TAC-TOE](https://en.wikipedia.org/wiki/Tic-tac-toe) game.

## What's in the repository

The repository contains just an **empty Unity project** ready for you to start to implement.
The project has been made with version **2021.3.11f1 (LTS)**, please work in a similar version (2021.3.x).

## Implementation

You need to take care of the **UI** and **user input** at first. Build the **game field**, and implement the logic needed for **turns and moves**.
The game has to implement both **single player** and **multiplayer**.
At any move you have to first **check the validity** of the move and then **check the result** (did it cause the game to end? with which outcome?).

For AI decision making and result check, **use API** as described below. This will require you to correctly *handle asynchronous operations*.

# API

We offer **two end-points** for you to get access to the game logic.

As a general concept, the **cells on the field** are identified by index as follows:

| 0 | 1 | 2 |
|--|--|--|
| 3 | 4 | 5 |
| 6 | 7 | 8 |

The **symbols** are represented as follows:
- '**_**': *(underscore)* represents an empty cell
- '**0**': *(zero)* represents the circle symbol
- '**1**': *(one)* represents the cross symbol

## Base URL

You can access API from the following base URL:
> ~~Deleted~~

## Authentication

Authentication will happen by adding an **header** named '*auth*' (no quotes) with the access token as value (right below).

Access token
> ~~Deleted~~

## Response

You will receive a response in **JSON format** with the following standard fields:

 - '**status**': a string field containing either 'error' or 'success'
 - '**message**': a string field containing further information about the status (usually empty in case of success)
 - '**data**': a JSON object based on the end-point or null on error

## Next Move
> /nextmove.php

### GET Parameters
#### field
A string of exactly 9 characters representing the field, one character per cell, starting from the top-left corner and growing left to right, row by row.

### Response Data
#### nextMove
A numeric value indicating the index of the cell for the next move.

### Common error causes

 - Invalid authentication
 - Missing or malformed 'field' get parameter
 - Field has no empty cells

### Example
> /nextmove.php?field=0_1_0_1_0
#### Response

    {
    	"status":  "success",
    	"message":  "",
    	"data":  {
    		"nextMove":  5
    	}
    }

## Result
> /result.php

### GET Parameters
#### field
A string of exactly 9 characters representing the field, one character per cell, starting from the top-left corner and growing left to right, row by row.

### Response Data
#### result
A string of exactly one character representing the outcome of the game, coded as follows:
- '**_**': *(underscore)* no result, the game is on
- '**x**': *(lower case x)*draw, no winner, no loser, no empty cell left
- '**0**': *(zero)* circle symbol won the game
- '**1**': *(one)* cross symbol won the game

### Common error causes

 - Invalid authentication
 - Missing or malformed 'field' get parameter

### Example
> /result.php?field=0_1_0_1_0
#### Response

    {
        "status": "success",
        "message": "",
        "data": {
            "result": "x"
        }
    }
