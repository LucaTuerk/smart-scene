# smart-scene

## Setup

To setup the smart-scene with your Unity project, clone this repository.
In the Unity package manager, click "Add package from disk" and select the package.json file.

## Usage
To open the smart-scene editor window, choose Tools/SmartScene/Settings from the taskbar.

### GridMesh Baking
First make sure a NavMesh is baked for the scene. (See Window/AI/Navigation)
Then open the GridMesh tab in the smart-scene window and set the vertex limit. 
The initial value should be fine for medium sized levels.
Then simply click Bake.

### Tools
#### Single Paths Tool
To use the single paths tool, open the (Tools/Path Lenght) tab in the smart-scene window.
Then add corner points in the scene to calculate the length of the connecting path.

#### Area Designation
To designate an area, go to the (Tools/Mark Area tab), click "Add Area" and mark the corners of the
area in the scene.

#### Marking Groups
To designate marking groups, open the (Tools/Mark/Marking Groups) tab and add a new group.
Vertices can be added by clicking the scene.

### Materials
Materials can be selected in the Materials via the dropdown menu. Some selected materials:
#### Two Team Distance Material
To use the two team distance material, first mark the respective spawn areas using marking groups.
Then select the two in the material and click bake.
To use distance values as vertex attributes in other materials enter a name for the attributes before baking.

Once the material is baked, a slider will appear, that can be used to change the threshold distance.
##### Additional options are:
* Show red, Show blue: Selectively turn of rendering for one team
* Overlap: Overlap colors to show advance beyond meeting points
* Meetingpoint: Show meeting point. The distance slider grows and shrinks the meeting area.

#### Area To Area Visibility
First mark your areas in the tools section, and select them  as A (source area) and B (target area) in the material. Here you can also choose to use the whole mesh or additional vertex groups.

Samples and penumbra samples then selects how many samples are used in each stage of the adaptive raytracing.
Penumbra Samples are automatically selected as a quarter of the samples.

Distinct target sets decides how many different target sets are generated for the source area.

Number of jobs selects, how many visibility values are calculated per frame. Increase for faster rendering times.

#### Value Range Material
First select a float attribute. If no dropdown menu exists, toggle "choose from existing attributes".
Then click "set selector MinMax from data" to adjust the range selector for the data. 
Then select your range and whether the minimum and maximum, name your vesrtex group and click bake.

#### Set Logic Material
First select your areas or vertex groups, and the intended logical operation.
Then name the resulting vertex group and bake.

