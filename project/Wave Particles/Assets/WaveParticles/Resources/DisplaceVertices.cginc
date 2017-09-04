#ifndef DISPLACE_VERTICES
#define DISPLACE_VERTICES
//TODO: Take x and z vertex displacement into account when calculating the normal at this position
inline float3 calculateNormal(float height_north, float height_east, float height_south, float height_west, float height_here,
							  float _UnitX, float _UnitY)
{
	float3 north_displacement = { 0, (height_north - height_here), _UnitY };
	float3 east_displacement = { _UnitX, (height_east - height_here), 0 };
	float3 south_displacement = { 0, (height_south - height_here), -_UnitY };
	float3 west_displacement = { -_UnitX, (height_west - height_here), 0 };

	float3 average_n = (cross(north_displacement, east_displacement) + cross(east_displacement, south_displacement) + cross(south_displacement, west_displacement) + cross(west_displacement, north_displacement));
	return normalize(average_n);
}


#endif