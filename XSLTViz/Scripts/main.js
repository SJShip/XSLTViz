$(function ()
{
	var
		w = document.documentElement.clientWidth,
		h = document.documentElement.clientHeight,
		canvas = d3.select("body").append("svg")
			.attr("width", w)
			.attr("height", h)
			.attr("viewBox", [0, 0, w * 3, h * 3].join(" ")),

		defs = canvas.append("defs");


	var initialWidth = w * 3;
	var initialHeight = h * 3;

	var zoomStep = 0.01;
	var zoomIn = function ()
	{
		var viewBoxParams = canvas.attr("viewBox").split(" ");
		var x = Number(viewBoxParams[0]);
		var y = Number(viewBoxParams[1]);
		var width = Number(viewBoxParams[2]);
		var height = Number(viewBoxParams[3]);

		var widthDelta = width * zoomStep;
		var heightDelta = height * zoomStep;

		viewBoxParams[0] = (x + widthDelta / 2.0).toString();
		viewBoxParams[1] = (y + heightDelta / 2.0).toString();
		viewBoxParams[2] = (width - widthDelta).toString();
		viewBoxParams[3] = (height - heightDelta).toString()
		canvas.attr("viewBox", viewBoxParams.join(" "));
	}

	var zoomOut = function ()
	{
		var viewBoxParams = canvas.attr("viewBox").split(" ");
		var x = Number(viewBoxParams[0]);
		var y = Number(viewBoxParams[1]);
		var width = Number(viewBoxParams[2]);
		var height = Number(viewBoxParams[3]);

		var widthDelta = width * zoomStep;
		var heightDelta = height * zoomStep;

		if (width + widthDelta <= initialWidth && height + heightDelta <= initialHeight)
		{
			viewBoxParams[0] = (x - widthDelta / 2.0).toString();
			viewBoxParams[1] = (y - heightDelta / 2.0).toString();
			viewBoxParams[2] = (width + widthDelta).toString();
			viewBoxParams[3] = (height + heightDelta).toString()
			canvas.attr("viewBox", viewBoxParams.join(" "));
		}
	}

	var adjustBoundaries = function (e)
	{
		var
			w = document.documentElement.clientWidth,
			h = document.documentElement.clientHeight;

		canvas
			.attr("width", w)
			.attr("height", h);
	}

	var mouseWheelHandler = function (e, delta)
	{
		e = e || window.event;
		var delta = e.deltaY || e.detail || e.wheelDelta;
		if (delta > 0)
		{
			zoomOut();
		} else
		{
			zoomIn();
		}
	}

	window.addEventListener("resize", adjustBoundaries);
	window.addEventListener("mousewheel", mouseWheelHandler);

	d3.json("api/graph/1", function (graph)
	{
		var force = d3.layout.force()
			.size([w, h])
			.charge(-120)
			.linkDistance(30)
			.nodes(graph.nodes)
			.links(graph.links)
			.start();

		//Create all the line svgs but without locations yet
		var link = svg.selectAll(".link")
			.data(graph.links)
			.enter().append("line")
			.attr("class", "link")
			.style("stroke-width", function (d)
			{
				return Math.sqrt(d.value);
			});

		//Do the same with the circles for the nodes - no 
		var node = svg.selectAll(".node")
			.data(graph.nodes)
			.enter().append("circle")
			.attr("class", "node")
			.attr("r", 8)
			.style("fill", function (d)
			{
				return color(d.group);
			})
			.call(force.drag);

		//Now we are giving the SVGs co-ordinates - the force layout is generating the co-ordinates which this code is using to update the attributes of the SVG elements
		force.on("tick", function ()
		{
			link.attr("x1", function (d)
			{
				return d.source.x;
			})
				.attr("y1", function (d)
				{
					return d.source.y;
				})
				.attr("x2", function (d)
				{
					return d.target.x;
				})
				.attr("y2", function (d)
				{
					return d.target.y;
				});

			node.attr("cx", function (d)
			{
				return d.x;
			})
				.attr("cy", function (d)
				{
					return d.y;
				});
		});
	});

	//var force = d3.layout.force()
	//	.size([w, h])
	//	.nodes(nodes)
	//	.links(links);
});