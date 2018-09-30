$(function ()
{
	if (!String.prototype.format)
	{
		String.prototype.format = String.prototype.f = function ()
		{
			var args = arguments;
			return this.replace(/\{\{|\}\}|\{(\d+)\}/g, function (m, n)
			{
				if (m === "{{") { return "{"; }
				if (m === "}}") { return "}"; }
				return args[n];
			});
		};
	}

	var entityMap = {
		'&': '&amp;',
		'<': '&lt;',
		'>': '&gt;',
		'"': '&quot;',
		"'": '&#39;',
		'/': '&#x2F;',
		'`': '&#x60;',
		'=': '&#x3D;'
	};

	function escapeHtml(string)
	{
		return String(string).replace(/[&<>"'`=\/]/g, function (s)
		{
			return entityMap[s];
		});
	}

	var
		w = document.documentElement.clientWidth,
		h = document.documentElement.clientHeight;

	var splitInstance = Split(['#left_pane', '#viewport', '#right_pane'], {
		sizes: [1, 98, 1]
	});

	var rightPane = $("#right_pane");
	var codePlaceHolder = $("#left_pane > pre");
	var codeTitle = $("#left_pane > span.title");
	var chkShowLeaves = $("#highlight_leaves");
	chkShowLeaves.on("change", function ()
	{
		project.settings.highlighted_leafs = this.checked;
		if (this.checked)
		{
			$(document.body).addClass("show_leaves");
		} else
		{
			$(document.body).removeClass("show_leaves");
		}
		updateProject();
	});
	var chkColorEdges = $("#color_direction");
	chkColorEdges.on("change", function ()
	{
		project.settings.color_direction = this.checked;
		if (this.checked)
		{
			$(document.body).addClass("color_edges");
			d3.selectAll("line.link").attr("style", function ()
			{
				var id = $(this).attr("id");
				return "stroke: url(#{0});".f(id.replace("l_", "gr_"));
			});
		} else
		{
			$(document.body).removeClass("color_edges");
			$("line.link").removeAttr("style");
		}
		updateProject();
	});

	var chkShowLabels = $("#show_labels");
	chkShowLabels.on("change", function ()
	{
		project.settings.show_labels = this.checked;
		if (this.checked)
		{
			$(document.body).addClass("show_labels");
		} else
		{
			$(document.body).removeClass("show_labels");
		}
		updateProject();
	});

	var chkShowSize = $("#show_size");
	chkShowSize.on("change", function ()
	{
		project.settings.show_size = this.checked;
		updateProject(function ()
		{
			loadProject(project.id + 1);
		});
	});

	var chkBundleEdges = $("#bundle_edges");
	chkBundleEdges.on("change", function ()
	{
		if (this.checked)
		{
			bundleEdges(project.id + 1);
		} else 
		{
			loadProject(project.id + 1);
		}
	});

	var btnBuildTreeView = $("#btnBuildTree");
	btnBuildTreeView.on("click", function ()
	{

	});

	var btnUndockNodes = $("#btnUndockNodes");
	btnUndockNodes.on("click", function ()
	{
		$("#mdlUndockNodes").modal("show");
	});
	var btnConfirmUndock = $("#btnConfirmUndock");
	btnConfirmUndock.on("click", function ()
	{
		$("#mdlUndockNodes").modal("hide");
		undockProjectNodes();
	});

	$("#selectView a.dropdown-item").on("click", function (e)
	{
		e.preventDefault();
		var projectId = $(this).attr("data-id");
		loadProject(projectId);
	});



	var project = null;

	var
		canvas = d3.select("#viewport > svg")
			.attr("width", w)
			.attr("height", h)
			.attr("viewBox", [0, 0, w, h].join(" ")),
		defs = d3.select("#viewport > svg > defs");

	//Set up the colour scale
	var color = d3.scale.category20();

	var viewBoxUpdateTimeout = null;


	var initialWidth = w * 3;
	var initialHeight = h * 3;

	var zoomStep = 0.1;
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
	};

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
	};

	var adjustBoundaries = function (e)
	{
		var
			w = document.documentElement.clientWidth,
			h = document.documentElement.clientHeight;

		canvas
			.attr("width", w)
			.attr("height", h);
	};

	var mouseWheelHandler = function (e)
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

		if (viewBoxUpdateTimeout)
		{
			clearTimeout(viewBoxUpdateTimeout);
		}

		viewBoxUpdateTimeout = setTimeout(updateProject, 1000);
	};

	var mouseDownHandler = function (e)
	{
		var tagName = e.target.tagName.toLowerCase();
		if (e.button === 0 && tagName !== "circle" && tagName !== "text" && tagName)
		{
			this.startX = e.clientX;
			this.startY = e.clientY;
			$(document.body).addClass("move");
			this.onmousemove = function (e)
			{
				var dx = e.clientX - this.startX;
				var dy = e.clientY - this.startY;
				var viewBoxParams = $(this).attr("viewBox").split(" ");
				viewBoxParams[0] = Number(viewBoxParams[0]) - dx;
				viewBoxParams[1] = Number(viewBoxParams[1]) - dy;
				$(this).attr("viewBox", viewBoxParams.join(" "));
				if (Math.abs(dx) > 0.001 || Math.abs(dy) > 0.001)
				{
					this.startX = e.clientX;
					this.startY = e.clientY;
				}
			};
		}
	};

	var mouseUpHandler = function (e)
	{
		if (e.button === 0)
		{
			$(document.body).removeClass("move");
			this.onmousemove = null;
			this.onmouseup = null;
		}
	};

	window.addEventListener("resize", adjustBoundaries);
	canvas[0][0].addEventListener("mousewheel", mouseWheelHandler);
	canvas[0][0].addEventListener("mousedown", mouseDownHandler);
	canvas[0][0].addEventListener("mouseup", mouseUpHandler);

	function angleToPoints(angle)
	{
		var segment = Math.floor(angle / Math.PI * 2) + 2;
		var diagonal = (1 / 2 * segment + 1 / 4) * Math.PI;
		var op = Math.cos(Math.abs(diagonal - angle)) * Math.sqrt(2);
		var x = op * Math.cos(angle);
		var y = op * Math.sin(angle);

		return {
			x1: x < 0 ? 1 : 0,
			y1: y < 0 ? 1 : 0,
			x2: x >= 0 ? x : x + 1,
			y2: y >= 0 ? y : y + 1
		};
	}

	function getGradientParams(d)
	{
		var dx = d.target.x - d.source.x;
		var dy = d.target.y - d.source.y;
		var angle = Math.atan2(dy, dx) - Math.atan2(0, 1);

		return angleToPoints(angle);
	}

	function updateFile(d)
	{
		$.ajax({
			method: "PATCH",
			contentType: "application/json",
			url: "api/files/" + d.id,
			data: JSON.stringify(d),
			error: function (e)
			{
				console.log(e);
			}
		});
	}

	function updateProject(onSuccess)
	{
		var settings = {
			viewbox: canvas.attr("viewBox"),
			highlighted_leafs: chkShowLeaves[0].checked,
			color_direction: chkColorEdges[0].checked,
			show_labels: chkShowLabels[0].checked,
			show_size: chkShowSize[0].checked
		};

		var ajaxOptions = {
			method: "PATCH",
			contentType: "application/json",
			url: "api/projects/{0}".f(project.id + 1),
			data: JSON.stringify(settings),
			error: function (e)
			{
				console.log(e);
			}
		};

		if (typeof onSuccess === "function")
		{
			ajaxOptions.success = onSuccess;
		}

		$.ajax(ajaxOptions);
	}

	function undockProjectNodes()
	{
		$.ajax({
			method: "PATCH",
			url: "api/projects/{0}/undock".f(project.id + 1),
			success: function ()
			{
				loadProject(project.id + 1);
			},
			error: function (e)
			{
				console.log(e);
			}
		});
	}

	function getFileContent(d)
	{
		codeTitle.text(d.name);
		$.ajax({
			method: "GET",
			url: "api/files/{0}/content".f(d.id),
			success: function (data)
			{
				btnBuildTreeView.show();
				codePlaceHolder.html(escapeHtml(data));
				codePlaceHolder.removeClass("prettyprinted");
				PR.prettyPrint();
				codePlaceHolder.show();
			},
			error: function (e)
			{
				console.log(e);
			}
		});
	}

	function dblclick(d)
	{
		$(".highlighted").removeClass("highlighted");
		$(".head").removeClass("head");

		d.fixed = false;
		d3.select(this).classed("fixed", d.fixed = false);
		updateFile(d);
	}

	function dragstart(d)
	{
		$(".highlighted").removeClass("highlighted");
		$(".head").removeClass("head");

		d3.select(this).classed("fixed", d.fixed = true);
	}

	function dragend(d)
	{
		$(".highlighted").removeClass("highlighted");
		$(".head").removeClass("head");

		var $this = $(this.childNodes[0]);
		d.fixed = true;
		d.x = Number($this.attr("cx"));
		d.y = Number($this.attr("cy"));
		updateFile(d);
		getFileContent(d);
	}

	function markRequiredElements(d)
	{
		$.ajax({
			method: "GET",
			url: "api/graph/{0}/path".f(d.id),
			contentType: "application/json",
			success: function (data)
			{
				$(".highlighted").removeClass("highlighted");
				$(".head").removeClass("head");

				$("g[data-id='{0}']".f(d.id)).addClass("head");

				data.nodes.forEach(function (node)
				{
					$("g[data-id='{0}']".f(node.id)).addClass("highlighted");
				});

				data.links.forEach(function (link)
				{
					$("#l_{0}_{1}".f(link.source, link.target)).addClass("highlighted");
				});


			},
			error: function (e)
			{
				console.log(e);
			}
		});
	}

	function bundleEdges(projectId)
	{
		document.force.stop();

		var line = d3.svg.line()
			.interpolate("bundle")
			.tension(.95)
			.x(function (d)
			{
				return d.x;
			})
			.y(function (d)
			{
				return d.y;
			});

		// Load project data
		$.ajax({
			method: "GET",
			url: "api/graph/{0}/trees".f(projectId),
			contentType: "application/json",
			success: function (trees)
			{
				map = {};

				links = [];

				var bundle = d3.layout.bundle();

				var root = { name: "", children: [] , x: 0, y: 0};


				trees.forEach(function (elem)
				{
					if (!map[elem.name])
					{
						map[elem.name] = elem;
					}
					var circle = $("g[data-id='{0}'] > circle".f(elem.id));
					var x = parseFloat(circle.attr("cx"));
					var y = parseFloat(circle.attr("cy"));
					elem.x = x;
					elem.y = y;
				});

				trees.forEach(function (elem)
				{
					if (elem.children && elem.children.length)
					{
						elem.children.forEach(function (importElem)
						{
							map[importElem].parent = elem;
							links.push({ source: elem, target: map[importElem] });
						});
					}
				});


				trees.forEach(function (elem)
				{
					if (!elem.parent)
					{
						elem.parent = root;
						root.children.push(elem);
					}
				});

				trees.unshift(root);

				//links = links.filter(l => l.source.parent.name != "" && l.target.parent.name != "");

				$("line.link", canvas[0][0]).css("display", "none");

				var stroke = d3.scale.linear()
					.domain([0, 1e4])
					.range(["brown", "steelblue"]);

				canvas.selectAll(".path")
					.data(bundle(links))
					.enter().append("path")
					.attr("class", "path")
					.attr("d", line)
					.style("stroke", function (d) { return stroke(d[0].value); });
					
			},
			error: function (e)
			{
				console.log(e);
			}
		});
	}

	function loadProject(projectId)
	{
		rightPane.css("visibility", "hidden");
		// Load project data
		$.ajax({
			method: "GET",
			url: "api/projects/" + projectId,
			contentType: "application/json",
			success: function (data)
			{
				project = data;
				$("#project_name").html(data.projectName);
				$("#total_files").html(data.totalFiles);
				chkShowLeaves.prop("checked", data.settings.highlighted_leafs);

				if (data.settings.highlighted_leafs)
				{
					$(document.body).addClass("show_leaves");
				} else
				{
					$(document.body).removeClass("show_leaves");
				}

				chkColorEdges.prop("checked", data.settings.color_direction);

				if (data.settings.color_direction)
				{
					$(document.body).addClass("color_edges");
				} else
				{
					$(document.body).removeClass("color_edges");
				}

				chkShowLabels.prop("checked", data.settings.show_labels);

				if (data.settings.show_labels)
				{
					$(document.body).addClass("show_labels");
				} else
				{
					$(document.body).removeClass("show_labels");
				}

				chkShowSize.prop("checked", data.settings.show_size);

				rightPane.css("visibility", "visible");

				buildGraphics(data);
			},
			error: function (e)
			{
				console.log(e);
			}
		});
	}

	function buildTreeView(fileData)
	{
		// Use default size for teh canvas
		canvas.attr("viewBox", "0 0 {0} {1}".f(w, h));

		d3.json("api/graph/treeview/{0}".f(fileData.id + 1), function (graph)
		{
			// Clears canvas data excepts <defs>
			canvas.selectAll("g.node").remove();
			canvas.selectAll("line.link").remove();
			canvas.selectAll("linearGradient[x1]").remove();

			document.force = d3.layout.force()
				.size([w, h])
				.charge(-50)
				.linkDistance(function (d)
				{
					return 0.1;
				})
				.nodes(graph.nodes)
				.links(graph.links)
				.start();

			// Create all the line svgs but without locations yet
			var link = canvas.selectAll(".link")
				.data(graph.links)
				.enter().append("line")
				.attr("class", "link")
				.attr("id", function (d)
				{
					return "l_{0}_{1}".f(d.source.id, d.target.id);
				})
				.style("stroke-width", function (d)
				{
					return Math.sqrt(d.value);
				});

			var linkGradient = defs.selectAll(".gr").data(graph.links).enter()
				.append("linearGradient")
				.attr("id", function (d) { return "gr_{0}_{1}".f(d.source.id, d.target.id); })
				.attr("xlink:href", "#color_edge");

			// Do the same with the circles for the nodes - no 
			var node = canvas.selectAll(".node")
				.data(graph.nodes)
				.enter().append("g")
				.attr("class", function (d)
				{
					var classes = ["node"];
					if (d.fixed)
					{
						classes.push("fixed");
					}
					return classes.join(" ");
				})
				.attr("data-id", function (d)
				{
					return d.id;
				})
				.append("circle")
				.attr("data-leaf", function (d)
				{
					return d.leaf;
				})
				.attr("r", function (d)
				{
					if (project.settings.show_size)
					{
						return d.size * 2;
					}
					return 3;
				})
				.select(function ()
				{
					return this.parentNode;
				})
				.append("text").text(function (d)
				{
					return d.name;
				})
				.attr("style", function (d)
				{
					if (project.settings.show_size)
					{
						return "font-size: {0}pt".f(d.size);
					}
				})
				.select(function ()
				{
					return this.parentNode;
				})
				.on("dblclick", dblclick)
				.on("mouseover", function (d)
				{
					if (event && event.shiftKey)
					{
						markRequiredElements(d);
					}
				})
				.call(drag);

			// Now we are giving the SVGs co-ordinates - the force layout is generating the co-ordinates which this code is using to update the attributes of the SVG elements
			document.force.on("tick", function ()
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

				linkGradient.attr("x1", function (d)
				{
					return getGradientParams(d).x1;
				})
					.attr("y1", function (d)
					{
						return getGradientParams(d).y1;
					})
					.attr("x2", function (d)
					{
						return getGradientParams(d).x2;
					})
					.attr("y2", function (d)
					{
						return getGradientParams(d).y2;
					});

				if (project.settings.color_direction)
				{
					link.attr("style", function (d)
					{
						return "stroke: url(\"#gr_{0}_{1}\");".f(d.source.id, d.target.id);
					});
				} else
				{
					link.attr("style", "");
				}

				node.select(function () { return this.childNodes[0]; })
					.attr("cx", function (d)
					{
						return d.x;
					})
					.attr("cy", function (d)
					{
						return d.y;
					})
					.select(function ()
					{
						return this.parentNode.childNodes[1];
					})
					.attr("x", function (d)
					{
						return d.x;
					})
					.attr("y", function (d)
					{
						return d.y + 0.5;
					});
			});
		});
	}

	function buildGraphics(projectData)
	{

		if (projectData.settings.viewbox)
		{
			canvas.attr("viewBox", projectData.settings.viewbox);
		}

		// Load graph data
		d3.json("api/graph/{0}".f(projectData.id + 1), function (graph)
		{
			// Clears canvas data excepts <defs>
			canvas.selectAll("g.node").remove();
			canvas.selectAll("line.link").remove();
			canvas.selectAll("linearGradient[x1]").remove();

			document.force = d3.layout.force()
				.size([w, h])
				.charge(-50)
				.linkDistance(function (d)
				{
					return 0.1;
				})
				.nodes(graph.nodes)
				.links(graph.links)
				.start();

			var drag = document.force.drag()
				.on("dragstart", dragstart)
				.on("dragend", dragend);

			// Create all the line svgs but without locations yet
			var link = canvas.selectAll(".link")
				.data(graph.links)
				.enter().append("line")
				.attr("class", "link")
				.attr("id", function (d)
				{
					return "l_{0}_{1}".f(d.source.id, d.target.id);
				})
				.style("stroke-width", function (d)
				{
					return Math.sqrt(d.value);
				});

			var linkGradient = defs.selectAll(".gr").data(graph.links).enter()
				.append("linearGradient")
				.attr("id", function (d) { return "gr_{0}_{1}".f(d.source.id, d.target.id); })
				.attr("xlink:href", "#color_edge");

			// Do the same with the circles for the nodes - no 
			var node = canvas.selectAll(".node")
				.data(graph.nodes)
				.enter().append("g")
				.attr("class", function (d)
				{
					var classes = ["node"];
					if (d.fixed)
					{
						classes.push("fixed");
					}
					return classes.join(" ");
				})
				.attr("data-id", function (d)
				{
					return d.id;
				})
				.append("circle")
				.attr("data-leaf", function (d)
				{
					return d.leaf;
				})
				.attr("r", function (d)
				{
					if (project.settings.show_size)
					{
						return d.size * 2;
					}
					return 3;
				})
				.select(function ()
				{
					return this.parentNode;
				})
				.append("text").text(function (d)
				{
					return d.name;
				})
				.attr("style", function (d)
				{
					if (project.settings.show_size)
					{
						return "font-size: {0}pt".f(d.size);
					}
				})
				.select(function ()
				{
					return this.parentNode;
				})
				.on("dblclick", dblclick)
				.on("mouseover", function (d)
				{
					if (event && event.shiftKey)
					{
						markRequiredElements(d);
					}
				})
				.call(drag);

			// Now we are giving the SVGs co-ordinates - the force layout is generating the co-ordinates which this code is using to update the attributes of the SVG elements
			document.force.on("tick", function ()
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

				linkGradient.attr("x1", function (d)
				{
					return getGradientParams(d).x1;
				})
					.attr("y1", function (d)
					{
						return getGradientParams(d).y1;
					})
					.attr("x2", function (d)
					{
						return getGradientParams(d).x2;
					})
					.attr("y2", function (d)
					{
						return getGradientParams(d).y2;
					});

				if (project.settings.color_direction)
				{
					link.attr("style", function (d)
					{
						return "stroke: url(\"#gr_{0}_{1}\");".f(d.source.id, d.target.id);
					});
				} else
				{
					link.attr("style", "");
				}



				node.select(function () { return this.childNodes[0]; })
					.attr("cx", function (d)
					{
						return d.x;
					})
					.attr("cy", function (d)
					{
						return d.y;
					})
					.select(function ()
					{
						return this.parentNode.childNodes[1];
					})
					.attr("x", function (d)
					{
						return d.x;
					})
					.attr("y", function (d)
					{
						return d.y + 0.5;
					})					;
			});
		});
	}

	loadProject(1);
});