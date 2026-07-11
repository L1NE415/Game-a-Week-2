# Everything: Wasteland Survival Truck

## Overview

This project is a seven-day game design challenge based on the theme **"Everything."**

The game is a closed-cabin survival prototype where the player is trapped inside a giant wasteland survival truck. The outside world cannot be trusted as a source of supplies. Everything the player needs to survive must come from systems inside the vehicle, and every action the player can take happens within the truck cabin.

The truck is not just transportation. It is the player's shelter, ecosystem, food source, water source, power source, and fragile life-support machine.

## Core Gameplay Loop

1. **Automatic resource production**
   - The rainwater filter produces water.
   - The greenhouse module grows food over time.
   - The solar array generates electricity and movement energy.

2. **Player survival needs**
   - Hunger decreases over time.
   - Hydration decreases over time.
   - Mental health is affected by hunger, thirst, darkness, noise, emergencies, and equipment failures.

3. **Cabin management**
   - The player moves between stations inside the truck.
   - The player collects produced resources, toggles systems, repairs damaged equipment, and responds to warnings.
   - Survival depends on keeping the vehicle's internal ecosystem balanced.

4. **Emergency interruptions**
   - Sandstorms block solar generation and threaten vehicle momentum.
   - Acid rain can contaminate the water system if the rain collector is left open.
   - Greenhouse crises reduce food output.
   - Mechanical failures can stop production or increase resource drain.

5. **Recovery and stabilization**
   - After each crisis, the player must restart systems, clean contamination, repair damage, and rebalance resource flow.
   - The main tension comes from returning the truck to a stable state before the next emergency begins.

## Key Systems

### Player Stats

- **Hunger**
  - Drops steadily over time.
  - Low hunger can reduce movement speed or interaction efficiency.

- **Hydration**
  - Drops steadily over time.
  - Low hydration can create health pressure or reduce player responsiveness.

- **Mental Health**
  - Drops when the player is hungry, thirsty, in darkness, under stress, or surrounded by failing systems.
  - Low mental health can cause input mistakes, distorted UI feedback, hallucinated warnings, or unreliable information.

### Vehicle Equipment

- **Rainwater Filter**
  - Produces water when rain is available.
  - During acid rain, leaving the collector open contaminates the water tank.

- **Greenhouse Module**
  - Slowly produces food.
  - Requires water and electricity to stay productive.

- **Solar System**
  - Produces electricity during clear weather.
  - Produces little or no power during sandstorms.

- **Drive System**
  - Converts electricity into vehicle movement.
  - If movement slows down, the truck stays inside dangerous weather for longer.

- **Repair Module**
  - Allows the player to fix damaged equipment.
  - Repairs cost time and may require spare parts.

## Emergency Events

### Sandstorm

**Effect:** Solar power generation drops sharply or stops completely.

**Player goal:** Reduce power usage, disable non-essential systems, prioritize the drive system, and escape the storm before the truck stalls.

### Acid Rain

**Effect:** Rainwater becomes dangerous. If the rain collector stays open, the water tank becomes contaminated.

**Player goal:** Shut down the rainwater collector quickly, isolate contaminated water if needed, and restore safe water access.

### Greenhouse Crisis

**Effect:** Crops may slow down, wither, or become infected.

**Player goal:** Allocate enough water and power, clear damaged crops, and decide whether to sacrifice the current harvest to protect the module.

### Vehicle Failure

**Effect:** One vehicle system stops working or begins consuming too many resources.

**Player goal:** Repair the failure while managing hunger, thirst, mental health, and other active problems.

## Seven-Day Challenge Scope

For the first playable version, the game should focus on one closed truck cabin and four to six core interactive stations.

The prototype should not include route planning, outside exploration, external scavenging, shops, destination nodes, or upgrade hubs.

The goal is a short survival run where the player either survives as many days as possible or endures a fixed-length wasteland journey using only what the truck can produce.

## Test Scenarios

- **Stable loop test**
  - With no emergency active, the player should understand how equipment production feeds into survival needs.

- **Sandstorm test**
  - When solar power is blocked, the player must manually manage energy to keep the truck moving.

- **Acid rain test**
  - If the player forgets to close the rainwater collector, water contamination should be obvious and meaningful.

- **Mental health test**
  - Hunger, thirst, darkness, and repeated emergencies should reduce mental health and increase pressure.

- **Full loop test**
  - A complete run should include resource production, player consumption, emergency interruption, player response, and recovery.

## Design Assumptions

- The player cannot leave the vehicle.
- The player cannot collect resources from the outside world.
- All survival resources come from vehicle systems.
- Vehicle equipment automatically produces food, water, and energy over time.
- The main design focus is not expansion or upgrades, but maintaining a fragile internal ecosystem under pressure.
